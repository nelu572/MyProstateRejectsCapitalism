# 프롤로그 인수인계 — 2026-11-05

## 다음에 열 씬 하나

**`Assets/Scenes/_Development/Prologue/Prologue_ClassroomToRestroom_Validation.unity`**

교실 기상 → 빈 교실 관찰 → 화장실 목적 → E키 미닫이문 → 밤 복도 → 측면 화장실 표식 → 작은 진입부의 문·500원 요금판·단말기 → E키 결제 시도 → 전립선 첫 만남·정체 공개 → F키 자유 대화까지 연결했다. 현재 시안의 최종 디자인 승인을 뜻하지 않는다.

기준 씬 수정·새 씬 생성·정리 절차는 [WORKFLOW.md](WORKFLOW.md)를 따른다.

## 안전과 복구

- 저장소 `C:/unity/MyProstateRejectsCapitalism`, 브랜치 `main`. 누적 구현 체크포인트 **`611921b`**, 역할별 정리 체크포인트 **`e37fa5c`**. 삭제한 과거 씬과 전용 에셋은 `e37fa5c`에서 Git/LFS로 복구할 수 있다. 최신 정리 커밋은 `git log -1`로 확인한다. 모두 로컬 개발 체크포인트다.
- 작업 전 `git status`, 현재 씬과 Unity MCP 프로젝트 경로를 확인한다. 이번 대상은 `MyProstateRejectsCapitalism@96a5e64b`, stdio 6400이었다.
- Editor 종료·재시작·종료 차단 우회 금지. 연결 문제는 기존 Editor를 유지하며 복구한다. 세부 규칙은 [AGENTS.md](../AGENTS.md), [WORKFLOW.md](WORKFLOW.md)를 따른다.
- 빌드 시작 씬·순서는 변경하지 않았다. 시작은 `Assets/Scenes/VerticalSlice/Slice_Act01_PaidToilet.unity`다. 현재 프롤로그는 Editor에서 직접 연다.

## 현재 Hierarchy

```text
_Player
  FirstPersonPlayer                 # Gameplay Prefab 연결 유지
_Gameplay
  WakeSequence                      # 기상·자막·위치 조건을 한 컴포넌트에서 관리
  TraversalBounds
  ACT1 Optional Conversation        # 첫 만남 종료 후 장소별 F 대화
_Environment
  Classroom/RoomAndDoor              # 내부 카메라·미닫이문 리깅 유지
  Corridor                          # 벽체, 창, 등기구, 시설, 연속 바닥·천장, 보강 메시
  SharedArchitecturalDetail          # 내부를 분리하지 않은 기존 모델 Prefab
  Exterior/CourtyardLighting         # 외부 모델과 결합된 조명은 함께 유지
  DistantBackdrop
  RestroomEntrance
_Lighting
  Environment
  LocalLights
  Probes
  Volumes/NightGrading
_UI                                 # 기존 자막·힌트·기상 페이드 Canvas 자체
```

공간과 결합된 교실·등기구·외부 조명은 억지로 `_Lighting`으로 분리하지 않았다. 꺼진 Renderer/Light와 시각 표면 아래의 Collider도 필요한 구성이므로 일괄 삭제하지 않는다. 월드 좌표를 사용하는 조건·원경 셰이더가 있어 공간 이동 시 함께 확인해야 한다.

## 주요 파일

| 용도 | 위치 |
| --- | --- |
| 현재 기상·독백 조건 | `Assets/Scripts/Gameplay/Prologue/ClassroomToRestroomPrologue.cs` |
| 기존 E키 문 | `Assets/Scripts/Gameplay/Prologue/ClassroomSlidingDoor.cs` |
| 게임용 / 기반 플레이어 | `Assets/Prefabs/Characters/FirstPersonPlayer_Gameplay.prefab`, `FirstPersonPlayer.prefab` |
| 학교 모델 | `Assets/Models/Environment/Prologue/` 아래 기존 모델별 폴더 |
| 프롤로그 재질·원경 셰이더 | `Assets/Materials/Prologue/` — `SchoolDarkContinuation/SoftNightSurface.shader` 포함 |
| 텍스처 | `Assets/Textures/Environment/Prologue/KoreanSchoolArt01/` |
| 현재 한글 폰트 | `Assets/Fonts/Prologue/PrologueClassroomToRestroomKorean.asset` |
| 현재 Volume | `Assets/Settings/Prologue/SchoolSubtleReveal/SubtleReveal.asset` |
| 제작 원본 | `ArtSource/Environment/`의 기존 디렉터리 관계 유지 |

`Models/Environment/RestroomLiminal`, `StartLiminal` 및 대응 재질은 ACT 1과 공유하므로 기존 위치를 유지했다. ArtSource 생성기의 Unity 출력 경로 5곳만 새 분류에 맞췄으며 생성기를 실행하지 않았다. `Logs`의 과거 생성·검증 코드는 당시 상태 기록이며, 옛 이름을 전제로 무작정 재실행하지 않는다.

## 유지한 플레이와 야간 화면

- A 설정: 속도 3.8m/s, 가속·감속·방향 전환 48/60/72, 중력 24, 점프 ON(0.65m), FOV 75°, 감도 0.10.
- 기상 약 6.2초, Space 건너뛰기, E 문 열기, Esc 커서 해제·클릭 재잠금을 유지했다.
- 복도 4m × 3m, 긴 직선과 심야 비주얼 유지. 형광등 1개 ON / 71개 OFF, 노출 0 EV. 공용 URP·Volume·조명 수치와 베이크는 변경하지 않았다.
- 교실 문은 z9.9, 측면 화장실 진입부는 z33.7(약 24m 뒤). 원경 메시 생략은 z78 이후, 배경은 z95 이후다. 기존 통행 한계 z53.7에서 멈추며 정상 동선은 그 안에 있다.
- 현재 씬의 배경은 이전 작업에서 복도 단면 안으로 제한한 상태다. 창밖에 곡면 외곽이 드러나지 않도록 유지한다.
- 독백은 관찰 시 `……다 갔네.`, 퇴실 의도 시 `화장실만 갔다 가자.`, 표식을 못 본 채 복도에서 40초·수평 16m 이상 탐색할 때만 `여기쯤 아니었나.`다. 빠른 진행에서는 앞 대사를 폐기하며 문 열림·표식 발견 직후는 무자막이다.

## 유지한 씬과 미통합 제작물

프로젝트 씬은 54개에서 10개로 줄였다. 프롤로그 외 유지 위치는 다음과 같다.

- ACT 1 참고: `Assets/Scenes/_Development/Act01_IntegratedValidation.unity`. 결제·첫 만남·자유 대화의 원본을 보존했다. 필요한 컴포넌트와 UI만 프롤로그에 재사용했다.
- 전체 진행 기반 및 ACT 2~5 기준: `Assets/Scenes/VerticalSlice/`의 5개 씬. 파일명·순차 전환·엔딩·빌드 순서를 유지했다.
- 독립 참고 2개: `_Development/FirstPersonFeelValidation.unity`(조작감·입력잠금 코스), `FullMapBlockoutV2.unity`(연속 전체 동선).
- `_Development/SampleScene.unity`: 현재 빌드 마지막 등록 때문에 보존했다. 개발 기준으로 사용하지 않는다.
- Lobby·Terminal·PaidToiletEntranceMinimal의 Prefab·모델·텍스처와 `ArtSource/` 제작 원본은 미통합 작업으로 남겼다.

과거 프롤로그 22개와 ACT별 통합 완료 검증·블록아웃 등 총 44개 씬은 삭제했다. `_Archive`에는 다시 복사하지 않았다. 사용이 끝난 전용 폰트·재질·프로필·베이크 데이터와 구형 독백 코드도 역참조 확인 후 제거했다. 공용 URP·Input·TMP Resources와 현재 씬의 의존 에셋은 유지했다.

`Scenes/_Development/Act01_AtmosphereValidation/`의 남은 ReflectionProbe 2개는 ACT 1 통합본의 공유 의존이므로 폴더 이름만 보고 삭제하지 않는다. 현재 프롤로그 Hierarchy·Prefab과 유지한 모든 씬의 내용은 이번 전체 정리에서 변경하지 않았다.

## 검증 자료와 보류

- 프롤로그 연결 자료: `Logs/PrologueAct1_20261105/`. 실제 1920×1080 Game View·FOV 75° 캡처 5장과 `PrologueAct1_Review.zip`이 있다. Input System의 키보드·마우스 이벤트로 이동·문 열기·결제·대화 입력을 전달했으며 위치 이동이나 대화 직접 실행으로 동선을 건너뛰지 않았다.
- 자연 기상(약 6.2초)과 Space 건너뛰기, 결제 중복 방지, 첫 만남 12줄의 입력 잠금·시점 허용·조작 복귀, 공개 전 F 차단·첫 만남 종료 후 해금, 화장실·복도·교실의 최초/반복 대화를 확인했다. Missing Script·교차 씬 참조·컴파일 오류·검증 중 콘솔 오류/경고는 0건이다. 자동 입력 검증은 초행 발견성과 연출의 자연스러움에 대한 최종 승인을 대신하지 않는다.

이전 프롤로그 정리 자료는 `Logs/PrologueCleanup_20261105/`, 이번 의존·삭제 매핑과 대표 화면은 `Logs/ProjectCleanup_20261105/`에 있다. Logs는 Git 제외다. 동일 내용의 캡처·JSON·ZIP만 유지 복사본과 SHA-256 매핑을 확인한 뒤 줄였고, 고유 캡처·최신 검토본·진행 중 로그는 보존했다.

- 사용자 미커밋 설정: `ProjectSettings/ShaderGraphSettings.asset`, `URPProjectSettings.asset`; 미추적 `SceneTemplateSettings.json`.
- 미추적 `Assets/Lightmaps/StartVisualReflection_ReflectionProbe.exr`와 `.meta`·폴더 `.meta`: 의도 미확인으로 삭제 보류.
- 고유 제작 원본·`.blend1`·서로 다른 백업 및 실패 기록, 유지 씬의 비활성 구조는 보존했다.
- 기존 문 대상 비교의 CS0252 경고는 연결 작업 중 참조 비교로 해소했다.
- 공용 `PC_Renderer`·`DefaultVolumeProfile`의 기존 미해결 GUID 11개는 삭제 대상과 무관하며 설정을 보존했다. ACT 1 재진입 중 기존 폰트의 임포터 경고 1회도 기록했다. 유지한 에셋의 파일 내용은 같고 최종 콘솔 오류·경고는 0건이었다.

## 미완료

작은 화장실 표식의 **초행 발견성과 대화 호흡은 사용자 플레이 검토 대기**다. 화장실 내부와 다음 ACT로의 이동은 아직 연결하지 않았다. 빌드 시작 씬·순서는 기존 그대로다.

## 결제·첫 만남 연결

- 기존 문·단말기 어셈블리에 `PaidToiletEvent`와 `ActOneFirstEncounter`를 붙였다. 문 Collider와 단말기 전용 Collider가 같은 이벤트를 사용하며 한 번만 실행된다. 실제 지불·문 개방 전 전립선이 제지하는 기존 흐름이다.
- `Assets/SO/ActOneIdentityReveal.asset`의 기존 12줄과 `Assets/SO/ActOneFreeConversation/`의 6개 대본을 그대로 참조한다. 기존 프롤로그 독백·기상 UI와 힌트를 유지하고 ACT 1의 대화 패널·대화 힌트만 가져왔다. 첫 만남 시작 즉시 프롤로그 독백을 종료한다.
- `ActOneFreeConversation`의 진행 씬 참조는 선택 사항으로 두었다. 프롤로그에는 다음 ACT 전환을 추가하지 않았으며, 장소 Bounds는 화장실 진입부 / 복도 / 교실(기존 NextArea 대화)로 지정했다. F키는 기존처럼 필수 첫 만남 전체 종료 후 열린다.
- 환경·건축·카메라·조명·Volume은 보존했다. 픽토그램 페인트의 대비와 프롤로그 전용 `SchoolPaymentScreen.mat`의 어두운 청록 화면만 소폭 보정했다. 공용 ACT 1 단말기 재질과 원본 씬·대본·플레이어 Prefab은 변경하지 않았다.
