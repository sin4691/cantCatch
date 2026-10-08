# CantCatch

손님 손을 아슬아슬하게 피하며 아이스크림을 파는 **터키 아이스크림 판매자 VR** 게임입니다.

| 장르 | 기간 | 인원 | 환경 |
|---|---|---|---|
| 1인칭 VR 캐주얼 | 2026.06.09 – 06.19 | 3인 | Unity · OpenXR · Final IK · Quest 3 (Link) |

📂 자세한 설명: [포트폴리오 – CantCatch](https://sin4691.github.io/#vr)

## 👀 신재윤 작업만 보기

3인 팀 프로젝트입니다. 아래 파일이 제가 만든 부분입니다. 비중은 현재 코드 기준 `git blame`으로 셌습니다.

| 기능 | 파일 | 비중 |
|---|---|---|
| 손님 팔 IK (LimbIK·거리 제한·1.5배 늘림) | [`CustomerArmIK.cs`](CantCatch/Assets/02.%20Scripts/Customer/CustomerArmIK.cs) | 87% |
| 손님 시선 (LookAtIK) | [`CustomerHeadLookAt.cs`](CantCatch/Assets/02.%20Scripts/Customer/CustomerHeadLookAt.cs) | 전부 |
| 손님 몸 이동 | [`CustomerBodyTracker.cs`](CantCatch/Assets/02.%20Scripts/Customer/CustomerBodyTracker.cs) · [`FollowTransform.cs`](CantCatch/Assets/02.%20Scripts/Customer/FollowTransform.cs) | 대부분 |
| 손님 상태·연출 | [`CustomerController.cs`](CantCatch/Assets/02.%20Scripts/Customer/CustomerController.cs) · [`CustomerState.cs`](CantCatch/Assets/02.%20Scripts/Customer/CustomerState.cs) | 전부 |
| 연타 미니게임 | [`MinigameManager.cs`](CantCatch/Assets/02.%20Scripts/Customer/MinigameManager.cs) · [`CustomerMashGame.cs`](CantCatch/Assets/02.%20Scripts/Customer/CustomerMashGame.cs) · [`CustomerAIInput.cs`](CantCatch/Assets/02.%20Scripts/Customer/CustomerAIInput.cs) | 대부분 |
| 스틱 손 고정·이동 범위 | [`HandGripController.cs`](CantCatch/Assets/02.%20Scripts/PlayerTools/HandGripController.cs) · [`StickMovementBounds.cs`](CantCatch/Assets/02.%20Scripts/PlayerTools/StickMovementBounds.cs) | 전부 |
| 점수 표시 | [`ScoreTextUI.cs`](CantCatch/Assets/02.%20Scripts/UI/ScoreTextUI.cs) | 전부 |

XR Origin·OpenXR 세팅과 VRIK 전신 아바타는 코드가 아니라 씬·프리팹 설정입니다. 시작 방향 맞춤은 [77ea9df](https://github.com/sin4691/cantCatch/commit/77ea9df) 커밋에 있습니다. [제 커밋만 모아 보기](https://github.com/sin4691/cantCatch/commits?author=sin4691)

## 게임 규칙

1분 동안 손님 손에 닿을 듯 말 듯 아이스크림을 피하면 점수를 얻습니다. 손에 닿으면 연타 대결이 시작되고, 이기면 콘만 내주고 지면 아이스크림을 빼앗깁니다.

## 내가 맡은 것

- **XR 플레이어**: XR Origin · OpenXR 세팅, VRIK로 헤드셋과 두 컨트롤러에서 판매자 전신 자세 계산, 시작 위치 이동 시 카메라와 아바타 방향 맞춤
- **손님 팔 IK** (`CustomerArmIK`): LimbIK 타깃이 콘을 따라가되 손님 앞쪽·최대 1m로 제한, 시간이 지날수록 추적 속도 상승, 팔이 짧으면 뼈 길이를 최대 1.5배까지 늘림
- **손님 시선** (`CustomerHeadLookAt`): LookAtIK로 콘을 바라봄
- **연타 미니게임** (`MinigameManager`, `CustomerMashGame`): 10초 동안 양쪽 연타를 한 게이지에 합산해 승패 결정, 결과에 따라 콘을 쥐거나 던지는 손님 연출

## 팀원 담당

손이 닿았는지 판정, 연타 게이지 UI, 게임 흐름 관리는 팀원 작업입니다.
