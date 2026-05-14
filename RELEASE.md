# 배포 방법

이 앱은 각 사용자 PC에 설치되고, 상담 데이터는 사용자별 AppData에 따로 저장됩니다.

데이터 위치:

```text
%AppData%\StudentCounseling\data.json
%AppData%\StudentCounseling\data.backup.json
```

따라서 GitHub Release나 설치 파일에는 상담 데이터가 포함되지 않으며, 설치한 사람끼리 DB를 공유하지 않습니다.

## 수동 배포

Windows에서 Inno Setup 6을 설치한 뒤 실행합니다.

```powershell
.\scripts\publish-installer.ps1 -Version 1.0.0
```

결과물:

```text
artifacts\installer\StudentCounseling_Setup_1.0.0.exe
```

## GitHub Release 자동 배포

GitHub에 코드를 올린 뒤 태그를 push합니다.

```powershell
git tag v1.0.0
git push origin v1.0.0
```

GitHub Actions가 Windows 설치 파일을 만들고, GitHub Release에 자동 첨부합니다.

## 설치 동작

- 앱 이름: 학생 상담 관리
- 설치 위치: 현재 사용자 LocalAppData 아래
- 관리자 권한: 필요 없음
- 바탕화면 바로가기: 기본 생성
- 데이터: 각 사용자 AppData에 별도 생성
