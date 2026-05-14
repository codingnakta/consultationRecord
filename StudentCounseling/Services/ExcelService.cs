using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using StudentCounseling.Models;

namespace StudentCounseling.Services;

public class ExcelImportResult
{
    public List<Student> Students { get; } = new();
    public List<string> RowErrors { get; } = new();
}

public class ExcelService
{
    public ExcelImportResult ImportStudents(string path)
    {
        var result = new ExcelImportResult();
        using var wb = new XLWorkbook(path);
        var ws = wb.Worksheets.First();
        var used = ws.RangeUsed();
        if (used is null) return result;

        var rows = used.RowsUsed().ToList();
        if (rows.Count <= 1) return result;

        // Header row
        var header = rows[0].Cells().Select(c => c.GetString().Trim()).ToList();
        int Col(params string[] names)
        {
            for (int i = 0; i < header.Count; i++)
                foreach (var n in names)
                    if (string.Equals(header[i], n, StringComparison.OrdinalIgnoreCase)) return i + 1;
            return -1;
        }
        int cName = Col("이름", "Name");
        int cGrade = Col("학년", "Grade");
        int cClass = Col("반", "Class", "ClassNumber");
        int cNum = Col("번호", "Number");
        int cMemo = Col("메모", "Memo");

        if (cName < 0 || cGrade < 0 || cClass < 0 || cNum < 0)
        {
            result.RowErrors.Add("헤더에 '이름/학년/반/번호' 컬럼이 필요합니다.");
            return result;
        }

        for (int i = 1; i < rows.Count; i++)
        {
            var r = rows[i];
            try
            {
                var name = r.Cell(cName).GetString().Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;
                var grade = (int)r.Cell(cGrade).GetDouble();
                var cls = (int)r.Cell(cClass).GetDouble();
                var num = (int)r.Cell(cNum).GetDouble();
                var memo = cMemo > 0 ? r.Cell(cMemo).GetString() : string.Empty;

                result.Students.Add(new Student
                {
                    Name = name,
                    Grade = grade,
                    ClassNumber = cls,
                    Number = num,
                    Memo = memo,
                });
            }
            catch (Exception ex)
            {
                result.RowErrors.Add($"{r.RowNumber()}행: {ex.Message}");
            }
        }

        return result;
    }

    public void ExportStudents(string path, IEnumerable<Student> students)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("학생");
        ws.Cell(1, 1).Value = "이름";
        ws.Cell(1, 2).Value = "학년";
        ws.Cell(1, 3).Value = "반";
        ws.Cell(1, 4).Value = "번호";
        ws.Cell(1, 5).Value = "메모";
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var s in students.OrderBy(s => s.Grade).ThenBy(s => s.ClassNumber).ThenBy(s => s.Number))
        {
            ws.Cell(row, 1).Value = s.Name;
            ws.Cell(row, 2).Value = s.Grade;
            ws.Cell(row, 3).Value = s.ClassNumber;
            ws.Cell(row, 4).Value = s.Number;
            ws.Cell(row, 5).Value = s.Memo;
            row++;
        }
        ws.Columns().AdjustToContents();
        wb.SaveAs(path);
    }

    public void ExportCounselings(string path, IEnumerable<Counseling> counselings, IReadOnlyDictionary<Guid, Student> studentLookup)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("상담 기록");
        var headers = new[] { "학년", "반", "이름", "집단명", "상담일", "상담시간", "소요(분)", "상담방법", "상담유형", "세부카테고리", "상담내용", "후속조치", "다음예약일" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;

        int row = 2;
        foreach (var c in counselings.OrderBy(c => c.Date).ThenBy(c => c.StartTime))
        {
            foreach (var sid in c.StudentIds)
            {
                if (!studentLookup.TryGetValue(sid, out var s)) continue;
                ws.Cell(row, 1).Value = s.Grade;
                ws.Cell(row, 2).Value = s.ClassNumber;
                ws.Cell(row, 3).Value = s.Name;
                ws.Cell(row, 4).Value = c.GroupName;
                ws.Cell(row, 5).Value = c.Date.ToString("yyyy-MM-dd");
                ws.Cell(row, 6).Value = $"{c.StartTime:hh\\:mm}~{c.EndTime:hh\\:mm}";
                ws.Cell(row, 7).Value = c.DurationMinutes;
                ws.Cell(row, 8).Value = c.Method.ToString();
                ws.Cell(row, 9).Value = c.Type.ToString();
                ws.Cell(row, 10).Value = c.SubCategory;
                ws.Cell(row, 11).Value = c.Content;
                ws.Cell(row, 12).Value = c.FollowUp;
                ws.Cell(row, 13).Value = c.NextDate.HasValue ? c.NextDate.Value.ToString("yyyy-MM-dd") : string.Empty;
                row++;
            }
        }
        ws.Columns().AdjustToContents();
        wb.SaveAs(path);
    }
}
