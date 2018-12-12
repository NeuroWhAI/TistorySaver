# Tistory Saver

[![Build status](https://ci.appveyor.com/api/projects/status/9rjv4op0rk9qlra4?svg=true)](https://ci.appveyor.com/project/NeuroWhAI/tistorysaver)

[OpenAPI](https://tistory.github.io/document-tistory-apis)를 사용하여 티스토리의 글을 파일로 백업합니다.

## Features

- 소유한 여러 블로그 선택 가능.
- 글의 내용만 HTML 형식으로 다운로드.
- 이미지, 첨부파일(티스토리 자체) 다운로드 및 HTML상에서의 경로 변경.
- 카테고리 구조를 폴더 구조로 유지.
- API 앱 변경 가능.

## How

1. 프로그램 실행 후 '인증' 버튼을 누릅니다.
   - 자신의 [API 앱](https://www.tistory.com/guide/api/manage/list)으로 인증하려면 해당 정보를 입력하고 진행하면 됩니다.
1. 웹 페이지가 열리면 티스토리 계정으로 로그인하고 만약 이어지는 페이지에서 '허가하기' 버튼이 나온다면 눌러줍니다.
1. 프로그램이 준비되면 대상 블로그를 선택합니다.
1. 저장할 폴더를 '찾기' 버튼을 눌러 선택합니다.
   - 여기서 지정하는 폴더 아래에 블로그 이름 폴더가 생깁니다.
   - 만약 지정한 폴더 아래 백업이 이미 존재한다면 그 글만 건너뛰게 됩니다.
1. '시작' 버튼을 눌러 백업을 시작합니다.
   - 에러가 발생한다면 다시 시도해봅니다.
1. 제대로 백업이 되었는지 확인하고 문제가 있다면 그 글에 해당하는 폴더만 지우고 다시 백업을 수행합니다.