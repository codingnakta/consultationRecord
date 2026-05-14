using System.Collections.Generic;

namespace StudentCounseling.Models;

public enum CounselingType
{
    학부모상담,
    개인상담,
    집단상담,
    심리검사,
}

public enum CounselingMethod
{
    대면,
    전화,
    기타,
    외부연계,
}

public static class CounselingCategories
{
    public static readonly Dictionary<CounselingType, string[]> SubCategories = new()
    {
        [CounselingType.학부모상담] = new[]
        {
            "학생관련상담", "교사관련상담", "학습", "기타",
        },
        [CounselingType.개인상담] = new[]
        {
            "학업", "진로", "성격", "성", "대인관계", "가정 및 가족관계",
            "일탈 및 비행", "학교폭력 가해", "학교폭력 피해",
            "자해 및 자살", "정신건강", "컴퓨터 및 스마트폰 과사용",
            "정보제공", "기타",
        },
        [CounselingType.집단상담] = new[]
        {
            "학업", "진로", "학교폭력", "성격,대인관계", "기타",
        },
        [CounselingType.심리검사] = new[]
        {
            "MBTI", "홀랜드", "MLST", "U&I", "다중지능", "기타",
        },
    };
}
