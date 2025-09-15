# SimpleTodoApp

Windows Forms 기반의 이모지 체크박스 TODO 앱입니다.

## 주요 기능
- 🟥/✅ 이모지 체크박스 표시
- 날짜 클릭 시 바로 수정 가능(DatePicker)
- Tab/Shift+Tab으로 최대 3단계 들여쓰기 지원
- 완료/미완료, 삭제, 편집, 정렬, 다크테마 UI
- 저장 경로(AppData/ProgramFiles) 옵션화

## 실행 방법
1. .NET 9.0 SDK 필요
2. `dotnet run` 또는 `dotnet publish` 후 실행

## 주요 파일
- `Form1.cs` : 메인 폼 및 이모지 체크박스/들여쓰기/날짜 클릭 등 UI
- `TodoItem.cs` : TODO 데이터 구조
- `TodoManager.cs` : 저장/불러오기/정렬 등 관리
- `EditTodoForm.cs` : 날짜/텍스트 편집 폼
