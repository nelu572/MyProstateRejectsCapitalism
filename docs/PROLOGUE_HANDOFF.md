# ACT 1 인수인계 — 2026-11-03

## 작업을 시작하기 전에

- 저장소: `C:/unity/MyProstateRejectsCapitalism`, 브랜치 `main`. 이 문서는 **현재 구현의 체크포인트**이며 프롤로그·환경 아트의 최종 승인 문서가 아니다.
- `git status --short`, `git log -5 --oneline`을 먼저 확인한다. 기존 미커밋 변경을 새 작업에 섞거나 되돌리지 않는다.
- Unity MCP에서 프로젝트 경로가 위 저장소의 `Assets`와 일치하는 Editor만 선택한다. 이번 확인 당시 `MyProstateRejectsCapitalism@96a5e64b`, stdio 포트 6400이었다. 다른 프로젝트 Editor도 연결되어 있으므로 이름·경로를 다시 확인한다.
- **Unity Editor를 임의로 종료·재시작하지 않는다.** `File/Exit`, `File/Quit`, `EditorApplication.Exit`, `Application.Quit`, 프로세스 강제 종료와 우회 실행 모두 금지한다. 연결 실패는 기존 프로세스를 유지하는 방법으로만 대응하고, 어려우면 해당 검증을 보류한다.
- 기존 임시 종료 차단을 제거하거나 우회하지 않는다. 메모리 차단은 도메인 재로드 시 소실될 수 있다. 종료 명령을 실행해 차단을 시험하지 않는다. 과거 MCP 로그에 실제 `File/Exit` 요청이 있었으나 원래 요청 주체는 확인되지 않았다.
- 상세 규칙: [AGENTS.md](../AGENTS.md), [WORKFLOW.md](WORKFLOW.md). 이 문서를 읽었다는 이유로 삭제·정리·커밋·Push가 자동 승인되는 것은 아니다.

## 프로젝트와 플레이 가능한 범위

- Unity **6000.0.68f1**. Input System **1.18.0**, URP **17.0.4**, uGUI/TMP **2.0.0**, Unity MCP **10.3.0**, Newtonsoft JSON **3.2.1**. 정확한 패키지 상태는 `Packages/manifest.json`과 `packages-lock.json`을 따른다.
- 최신 ACT 1 검토 시작 씬: **`Assets/Scenes/_Development/Act01_IntegratedValidation.unity`**.
- 기존 빌드 시작 씬: `Assets/Scenes/VerticalSlice/Slice_Act01_PaidToilet.unity`. **통합 씬으로 교체하지 않았다.** 통합 씬은 Editor에서 직접 Play하며 기존 ACT 2로 전환한다. 빌드 목록에는 기존 ACT 1~5와 SampleScene이 들어 있다.
- 기존 Vertical Slice는 유료 화장실 → 변기 전시장 → 광고 통로 → 무료 우회로 → 최종 무료 화장실·페이드·엔딩·재시작의 기능적 기반이다. 최신 ACT 1 아트·자유 대화·선택 조작감이 모든 ACT에 반영된 것은 아니다.
- ACT 2~5 씬: `Assets/Scenes/VerticalSlice/Slice_Act02_Showroom.unity`, `Slice_Act03_Advertising.unity`, `Slice_Act04_Detour.unity`, `Slice_Act05_FinalToilet.unity`.
- 진행은 `SliceActFlow`/`SliceExit`의 E 상호작용과 짧은 페이드로 연결된다. 필수 조사·대화가 끝나야 출구가 열린다. 저장·로드나 복잡한 전역 진행 시스템은 없다.

## 최신 ACT 1의 실제 의존 관계

`Act01_SelectedPlayerValidation`의 최신 결과를 복제한 통합본이다. 과거 씬 오브젝트를 중복 합치지 않았다. 메시·재질·조명·Volume·UI 참조가 최신 원본과 일치함을 확인했다.

| 용도 | 현재 참조 |
| --- | --- |
| 플레이어 | `Assets/Prefabs/Characters/FirstPersonPlayer_Gameplay.prefab` |
| 기반 프리팹 | `Assets/Prefabs/Characters/FirstPersonPlayer.prefab` — 게임용 파생 프리팹의 원본 |
| 이동·상호작용 | `Scripts/Characters/FirstPersonPlayer.cs`, `Scripts/Interaction/PlayerInteractor.cs`, `IInteractable.cs` (모두 `Assets/` 아래) |
| 독백·첫 만남·자유 대화 | `Assets/Scripts/Gameplay/ActOnePrologue.cs`, `ActOneFirstEncounter.cs`, `ActOneFreeConversation.cs` |
| 자막·대사 데이터 | `Assets/Scripts/Dialogue/DialogueController.cs`, `DialogueSequence.cs`, `Assets/SO/ActOneIdentityReveal.asset`, `Assets/SO/ActOneFreeConversation/` 6개 |
| 화장실 이벤트 | `Assets/Prefabs/Interactables/PaidToilet.prefab`, `Assets/Scripts/Gameplay/PaidToiletEvent.cs` |
| 시작·발견 공간 | `Assets/Models/Environment/StartLiminal/`, `RestroomLiminal/`, `RestroomConnections/`의 실제 참조 메시, 대응 `Assets/Materials/`·`Textures/` |
| 자막 마감 | `Assets/Materials/IdentityReveal/`, `Assets/Textures/IdentityReveal/SubtitleVeil.png` |
| 조명 | `Assets/Settings/Act01Atmosphere/AtmosphereVolume.asset`, `Assets/Settings/ActOneIntegrated/InheritedLighting.lighting`, 통합 씬과 같은 이름의 베이크 폴더 |
| 재사용 반사 데이터 | `Assets/Scenes/_Development/Act01_AtmosphereValidation/ReflectionProbe-0.exr`, `ReflectionProbe-1.exr` — 과거 씬 폴더 안에 있어도 **현 통합 씬이 사용**하므로 삭제 금지 |
| 편집·재생성 원본 | `ArtSource/Environment/StartLiminal/`, `RestroomLiminal/`, `RestroomConnections/`의 제작 Python·메시 JSON·통합 소스 |

한글 TMP 폰트는 `Assets/Fonts/ActOnePrologueKorean.asset`, `PrototypeKorean.asset`, `VerticalSliceKorean.asset`를 사용한다. Dynamic OS 폰트 **Malgun Gothic**을 참조하며 원본 경로는 `C:/WINDOWS/Fonts/malgun.ttf`이다. 저장소에 이 시스템 TTF는 없다. 다른 OS에서 새 글리프를 생성할 때는 폰트 가용성을 확인해야 한다. 이번 정리에서 폰트를 교체하지 않았다.

현재 카메라 FOV 등은 게임용 파생 프리팹에서 상속하며 선택 수치를 덮어쓰는 씬 Override는 없다. 공통 원본·비교 프리팹과 옛 실험 씬은 그대로다. 옛 씬의 B 설정·FOV 80 등의 Override를 최신 설정으로 오인하지 않는다.

## 선택한 조작감

| 항목 | 값 |
| --- | --- |
| 이동 프리셋 | A, CharacterController / New Input System 기반 |
| 이동 속도 | 3.8 m/s |
| 가속 / 감속 / 방향 전환 가속 | 48 / 60 / 72 m/s² |
| FOV | 75° |
| 점프 | ON, 높이 0.65 m, 중력 24 m/s² |
| 마우스 감도 | **0.10, 잠정값** |

일반 탐색·F 자유 대화에서는 이동·시점·점프를 허용한다. 필수 대화에서는 이동·점프를 차단하고, 당황하는 일부 대사에서 시점만 허용한다. 종료 후 입력을 복구한다. 기존 Esc 커서 해제·클릭 재잠금과 E/클릭 대사 진행을 유지한다. 통합 씬에는 비교용 `PlayerFeelComparison`, F1~F9·J 설정 전환, 개발 표시가 없다. 비교 환경은 `FirstPersonFeelValidation`, `Act01_FreeConversationFeelValidation`, `FirstPersonPlayer_Comparison.prefab`에 남아 있다.

## 캐릭터·대화와 검증 상태

기준: [Notion 기획서](https://app.notion.com/p/40ab3e6e776c8042affad02901733c91) **9. 캐릭터성 및 대화 연출 방향**. 후보와 확정 지시는 구분하고 Notion은 임의 수정하지 않는다.

- 주인공: 평범하고 감정적인 사람. 급한 화장실 문제를 해결하려 하며, 말하는 전립선에 진짜로 당황하고 믿지 못한다. 자연스러운 일상 반말을 쓴다.
- 전립선: 자신의 발화를 당연하게 여기며 결제 거부에 진지하고 고집스럽다. 가끔 허점을 드러내지만 냉소적 드립·철학적 명언을 반복하는 인물이 아니다.
- 현재 독백은 `아…… 화장실 가고 싶다.` → `어디 없나……` → `아, 저기 있다.` → `오백 원? 뭐, 됐어.` 순서로 위치·발견에 따라 자동 표시된다. 이동은 막지 않는다.
- 결제 반응 → `???`의 `잠깐.` → 주인공의 당황 → 정체 공개 → 500원 결제 갈등까지 12줄의 첫 만남이 구현되어 있다. 정체 공개를 확인한 뒤 `전립선` 표기로 바뀐다. 작은 하단 자막과 `›` 표시·짧은 페이드·기존 미세한 첫 등장 효과를 유지한다.
- 첫 필수 대화 종료 후 F가 해금된다. 유료 화장실 앞 / 정체 공개 이후 복도 / 다음 구역 직전의 대화는 각 4줄, 반복 응답은 각 2줄이다. 필수 진행 조건이 아니며 위치 진입·해금 시 F 안내가 잠깐 나온다. 대화 중 E 오브젝트 상호작용·씬 전환은 예약해 중복 실행을 막는다.
- 기존 사용자 피드백: 공간 구도와 발견 경험이 밋밋했고, 큰 검은 대화 패널·개발용 안내가 화면을 가렸으며, 두 인물의 말투·감정 차이가 약했다. 개별 실험에서 개선했으나 프롤로그·환경 아트를 최종 확정한 것은 아니다. 조작감은 사용자 선택으로 A/75°를 채택했고 감도 0.12가 빨라 0.10으로 낮췄다.
- 통합 씬에서 **시작부터 직접 걸어 ACT 2 진입까지 연속 플레이**를 확인했다. 독백 4개·필수 대사·이동·점프·시점·차단/복구가 동작했다. 핵심 플레이어·대화·진행 컴포넌트는 각각 1개, 누락 스크립트는 0개였다.
- 남은 검증 한계: 첫 자동검증은 자유 대화 도중 출구 영역에 들어간 것을 놓쳐 반복 대사 기대값을 잘못 비교했다. 검증 동선을 고친 재실행은 화장실 앞 반복 응답 첫 줄에서 PlayMode가 종료되어 완료하지 못했다. **세 위치의 첫/반복 대화 최종 확인을 전부 통과했다고 취급하지 않는다.** 사용자 종료인지 원인은 단정하지 않는다.
- 이번 Git 정리는 새 PlayMode·전체 회귀 테스트를 실행하지 않았다. 필수 씬과 의존 에셋의 GUID 참조를 확인했고, 기존 TMP 임포터/MCP 중복 전송 경고는 게임 오류와 구분한다.

## 파일 분류와 로컬 체크포인트

정리 전 HEAD는 `f521387`(Assets 폴더 구조 정리)였다. 이후 작업은 대부분 미추적 상태였다. 이번에는 파일 이동·삭제·GUID 재생성 없이 명시적 파일 목록으로 커밋을 분리했다. **A는 현재 참조·사용 중이라는 뜻이며 아트의 최종 승인 뜻이 아니다.**

| 분류 | 처리 |
| --- | --- |
| A — 현재 사용할 구현 | ACT 1 통합 씬, 게임용/기반 플레이어, 필수 런타임 코드·대사, 기존 Vertical Slice 5개 씬과 BuildSettings, 실제 참조 메시·재질·텍스처·폰트·베이크 및 제작 원본을 기능 체크포인트에 보존 |
| B — 미확정 후보 | 과거 블록아웃·V2, 터미널/로비 Blender·FBX, 과거 시작·화장실·UI·조명·조작감 검증 씬과 그 의존 에셋을 **미확정 실험 보존 커밋**에 별도 저장. 이후 작업의 기준 씬으로 자동 채택하지 않음 |
| C — 지원 파일 | Unity MCP manifest/lock, TMP 필수 리소스·라이선스, LFS 설정을 지원 커밋에 저장. AGENTS/WORKFLOW의 Editor 안전 지침과 본 문서는 문서 커밋으로 분리 |
| D — 임시·생성 후보 | `Logs/`의 캡처·ZIP·검증 로그·백업(이번 확인 약 1.8 GiB), 무시되는 `.blend1` 3개, Unity 반입본과 내용이 같은 `ArtSource` OBJ 4개, 모든 개발 씬 의존 조사에서 미참조인 `Assets/Lightmaps/StartVisualReflection_ReflectionProbe.exr`를 **로컬에 그대로 보존**. 미참조만으로 삭제를 승인하지 않음 |

로컬 체크포인트:

- 지원 리소스: `1845213` — 85개 파일, MCP/TMP/LFS 설정
- 현재 구현과 의존 에셋: `a18816d` — 260개 파일, ACT 1 통합 검증본과 Vertical Slice 기반
- 미확정 실험·편집 원본 보존: `122146b` — 444개 파일, 완성본 승인과 별개인 보존 체크포인트
- 이 문서와 안전 지침 커밋은 `git log -1`로 확인한다. 자신의 커밋 해시는 문서에 넣지 않는다.

커밋하지 않고 보호한 설정: `ProjectSettings/ShaderGraphSettings.asset`(`overrideShaderVariantLimit: 0` 추가), `ProjectSettings/URPProjectSettings.asset`(material version 9→10), `ProjectSettings/SceneTemplateSettings.json`(미추적 Editor 설정). 출처를 단정하지 않았으며 게임 변경과 섞지 않았다. 새 작업 시작 시 상태를 다시 확인하고 무단 포함·되돌리기 금지.

대용량 제작 파일은 새 `.gitattributes`로 LFS 추적한다. 폰트 atlas `.asset`, FBX/OBJ·Blender, 텍스처 PNG, EXR·베이크 바이너리가 대상이며 기존 Git 기록은 마이그레이션하지 않았다. 최대 개별 파일은 약 7.1 MiB였다. **커밋과 LFS 객체는 로컬에만 있고 Push하지 않았다.** 다른 PC로 옮길 경우 Git 커밋만으로 로컬 LFS 객체·미추적/무시 파일까지 백업된다고 생각하지 않는다. `.git/lfs/objects`와 보존 대상 작업 파일을 포함해야 한다.

상세 파일 분류·의존 목록은 로컬 `Logs/Handoff/classification.tsv`, `dependencies.tsv`에 있다(Logs는 Git 제외). Git 체크포인트의 실제 포함 파일은 `git show --stat <커밋>`으로 확인할 수 있다. 통합 씬과 빌드 목록의 의존 에셋 119개에서 GUID 132개를 해석했으며 미해결 참조는 없었다. 모든 개발 씬의 Unity 의존 파일도 Git에 포함되어 있다. `.meta` GUID 중복은 없고 기존 파일의 바이트는 보존했다. LFS 객체·포인터 무결성 검사를 통과했다. Unity 생성 파일의 기존 공백 경고는 무관한 포맷 변경을 피하기 위해 유지했다. 신규 코드 변경·재베이크·프로젝트 업그레이드는 이번 정리에서 하지 않았다.

## 다음 작업 — 새로운 프롤로그의 독립 블록아웃

**이번 인수인계 작업에서는 구현하지 않았다.** 다음 사용자 요청의 방향은 기존 복도를 더 늘리는 것이 아니라 새로운 프롤로그 공간을 실험하는 것이다.

1. 주인공은 기억하지 못하는 정체불명의 공간에서 눈을 떴다. 이미 수십 분 동안 화장실을 찾고 있었다.
2. 공간의 정체와 자신이 어떻게 왔는지는 불명확하다. 구체적인 배경 설명·대사·공간 구조는 아직 미정이다.
3. 넓거나 긴 리미널 공간을 탐색하며 자연스러운 독백으로 상황을 전한다. 독백이 공간 관찰을 방해하지 않게 한다.
4. 전립선의 존재는 첫 유료 화장실 이벤트 전까지 공개하지 않는다.
5. 기존 ACT 1과 원본·실험 후보를 보존하고 **독립적인 새 프롤로그 실험**으로 진행한다. 먼저 레벨 디자인·기본 도형 블록아웃에서 첫인상·동선·시야 변화를 검증한 뒤 환경 아트를 제작한다.

## 계속 적용할 제약

- 게임잼 마감 **2026-11-11(KST)**. 기능 범위를 최소화하고 기존 이동·상호작용·자막·이벤트를 재사용한다. 불필요한 리팩터링·범용 프레임워크·작업 일지 확대 금지.
- 사용자 승인 전 원본·실험 씬·Blender·FBX·편집 소스·미커밋 설정을 삭제하거나 대규모 이동하지 않는다. 폴더 이름만 보고 과거 시안의 의존 리소스를 제거하지 않는다.
- Unity Scene/Prefab/Inspector 작업은 MCP를 우선하며 `.unity`·`.prefab`을 직접 편집하지 않는다. C# 수정은 필요한 범위만 직접 편집할 수 있다.
- Unity Editor 임의 종료·재시작 금지. MCP 장애를 해결하기 위해 다른 Editor에서 작업하거나 별도 HTTP MCP 서버를 임의 실행·종료하지 않는다.
- 기존 Notion 기획서를 임의 수정하지 않는다. 기존 메인 씬 교체·다른 ACT 확장·커밋·Push는 그 작업에 대한 사용자 지시 없이 진행하지 않는다.
- 스크린샷을 만들면 실제 게임 화면의 **개별 PNG와 전체 ZIP**을 함께 제공한다.
