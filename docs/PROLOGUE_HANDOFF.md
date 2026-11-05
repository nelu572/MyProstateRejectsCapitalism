# 프롤로그 인수인계 — 2026-11-05

## 다음에 열 씬 하나

**`Assets/Scenes/_Development/Prologue/Prologue_ClassroomToRestroom_Validation.unity`**

교실 기상 → 빈 교실 관찰 → 화장실 목적 → E키 미닫이문 → 밤 복도 → 측면 화장실 표식 → 작은 진입부의 문·500원 요금판·단말기까지다. 결제·전립선 첫 만남·ACT 연결은 아직 없다. 현재 시안의 최종 디자인 승인을 뜻하지 않는다.

작은 수정은 이 씬과 Git 체크포인트를 사용한다. 매번 Validation 씬을 복제하지 않는다. 별도 씬은 실제 비교 실험이 필요할 때만 만든다.

## 안전과 복구

- 저장소 `C:/unity/MyProstateRejectsCapitalism`, 브랜치 `main`. 정리 전 보존 커밋: **`611921b`**. 최신 정리 커밋은 `git log -1`로 확인한다. 두 커밋은 로컬 개발 체크포인트이며 Push하지 않았다.
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
| 보관 씬용 예전 독백 코드 | 같은 폴더의 `ClassroomWakePrologue.cs`, `PrologueBlockoutThoughts.cs` |
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

## 보관과 기능 참고

- 과거 프롤로그 22개 씬: **`Assets/Scenes/_Development/_Archive/Prologue/`**. 파일명·GUID·각 후보의 의존 에셋을 보존했다.
- 기존 첫 만남·자유 대화 참고: `Assets/Scenes/_Development/Act01_IntegratedValidation.unity`.
- 전체 진행 기반: `Assets/Scenes/VerticalSlice/`의 ACT 1~5. 이름·연결·빌드 목록을 유지했다. 현재 프롤로그와 새로 연결하지 않았다.

## 정리·검증 자료와 보류

현재 씬에서 교체된 비활성 독백 컴포넌트 2개, 비활성 준비물 텍스트, 미참조 구형 트리거만 제거했다. 외부 참조가 없는 `Assets/Editor/TutorialInfo/` 템플릿을 정리했다. 캡처 복사본 15개는 동일 해시와 복구 ZIP을 검증한 뒤 제거했으며 원본·최신 캡처·실험별 차이·제작 원본은 보존했다.

정리 검증: 교실·입구 화면은 RGB 픽셀 동일, 복도는 1픽셀에서 1단계 차이였다. 유지한 3,427개 객체의 상태·직렬화 참조와 215개 이동 GUID를 보존했고, 54개 씬에서 누락 GUID가 없었다. 정리 후 한 번의 연속 Play에서 기상 도중 건너뛰기·조작권 반환·문·발견·독백·이동·점프·충돌을 확인했으며 새 경고·오류는 0건이었다.

정리 전후 캡처·객체 상태·의존 GUID·삭제 매핑과 복구 ZIP은 `Logs/PrologueCleanup_20261105/`에 있다. Logs는 Git 제외이며, 다른 PC로 옮길 때 Git만으로 보존된다고 생각하지 않는다. LFS 객체도 로컬에 보존해야 한다.

다음은 의도를 확정하지 않아 유지했다.

- 미커밋 설정: `ProjectSettings/ShaderGraphSettings.asset`, `URPProjectSettings.asset`; 미추적 `SceneTemplateSettings.json`.
- 미추적 `Assets/Lightmaps/StartVisualReflection_ReflectionProbe.exr`와 `.meta`·폴더 `.meta`: 보관 씬 의존에서도 사용을 확인하지 못했으나 삭제 보류.
- 미참조 Sky 재질, 꺼진 과거 구조, `.blend1`, 서로 다른 시점의 백업·실패 로그: 이름이나 미참조만으로 삭제하지 않았다.

## 미완료

작고 어두운 화장실 표식의 **초행 발견성은 사용자 플레이 검토가 필요**하다. 요금 결제·화장실 내부·전립선 첫 만남·기존 ACT 1~5 연결은 이번 범위 밖이다. 정리 후 새 기능이나 아트 변경은 하지 않았다.
