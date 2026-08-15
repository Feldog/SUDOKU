# Unity AI Agent Rules

## 프로젝트 정보
- Engine: Unity 6000.3.14f1
- Render Pipeline: URP
- Target Platform: Mobile
- Language: C#
- Main Folder: `Assets/`

---

## 기본 원칙

- 작업 범위 밖의 파일은 수정하지 않는다.
- 불필요한 리팩터링을 하지 않는다.
- 기존 public API는 요청 없이 변경하지 않는다.
- 기능 추가 시 기존 Inspector 설정이 깨지지 않게 한다.
- 변경 이유가 큰 구조 변경이라면 먼저 설명한다.

---

## Priority
When rules conflict, follow this order:

1. Preserve Unity references, GUIDs, and serialization.
2. Do not modify files outside requested scope.
3. Preserve existing architecture unless requested.
4. Prefer mobile performance and URP compatibility.

---

## Unity 안전 규칙

- `.meta` 파일을 수정하거나 재생성하지 않는다.
- Scene 파일을 요청 없이 수정하지 않는다.
- Prefab 구조를 요청 없이 변경하지 않는다.
- GUID 참조를 유지한다.
- serialized field 이름을 함부로 변경하지 않는다.
- `ProjectSettings/`는 요청이 있을 때만 수정한다.
- Unity YAML 파일을 직접 수정하지 않는다.

---

## 폴더 규칙

현재 프로젝트는 `Assets/` 아래의 숫자 기반 폴더 구조를 기준으로 관리한다.

- Scene: `Assets/01.Scenes/`
- 일반 스크립트: `Assets/02.Scripts/`
- 공통 스크립트: `Assets/02.Scripts/Commons/`
- Prefab: `Assets/03.Prefabs/`
- Animation: `Assets/04.Animation/`
- Material: `Assets/05.Materials/`
- ScriptableObject 에셋: `Assets/06.ScriptableObject/`
- UI Toolkit 루트: `Assets/07.UI_Toolkit/`
- UI Toolkit UXML: `Assets/07.UI_Toolkit/UXML/<기능명>/`
- UI Toolkit USS: `Assets/07.UI_Toolkit/USS/<기능명>/`
- UXML과 USS는 같은 폴더에 혼합하지 않고 파일 형식별 폴더로 분리한다.
- UI Toolkit 파일은 UXML과 USS 각각에서 동일한 기능명 하위 폴더를 사용한다. 예: `UXML/Room/`, `USS/Room/`
- 에디터 전용 UI Toolkit 파일은 기능 폴더의 `Editor/` 아래에 배치한다. 예: `UXML/DialogueSystem/Editor/`, `USS/DialogueSystem/Editor/`
- 에디터 윈도우 전용 스크립트 및 데이터: `Assets/98.EditorScript/<기능명>/`
- 에디터 윈도우 관련 파일은 기능별 하위 폴더로 구분한다. 예: `Assets/98.EditorScript/DialogueSystem/`
- `UnityEditor`에 의존하여 런타임 빌드에서 제외해야 하는 코드는 각 기능 폴더의 `Editor/` 아래에 배치한다. 예: `Assets/98.EditorScript/DialogueSystem/Editor/`
- 프로젝트 전용 Resources: `Assets/99.Resources/`

프로젝트 외부 에셋과 Unity 기본/패키지성 리소스는 기존 위치를 유지한다.

- Addressables 설정: `Assets/AddressableAssetsData/`
- 외부 에셋: `Assets/External Assets/`
- 플러그인: `Assets/Plugins/`
- Unity Resources: `Assets/Resources/`
- URP/프로젝트 렌더 설정 에셋: `Assets/Settings/`
- TextMeshPro 리소스: `Assets/TextMesh Pro/`

새 파일을 만들 때는 위 구조를 우선 따르고, 기존 기능과 같은 도메인의 파일은 같은 하위 폴더에 배치한다.

---

## C# 코드 스타일

- public class, method, property: PascalCase
- enum 타입명은 Enum임을 명시하기 위해 대문자 `E` 접두사를 붙인 PascalCase로 작성한다.
- enum 전용 파일은 대상 도메인의 `Enum/` 폴더에 배치하고 namespace도 `.Enum`으로 끝나게 작성한다.
- private field: camelCase
- private Inspector field는 `[SerializeField]` 사용
- `System`, `UnityEngine`, 외부 패키지 namespac은e는 파일 상단에 작성하고, 프로젝트 내부 같 도메인의 namespace는 namespace 블록 내부에서 축약 using을 우선 사용한다.
- 예: `namespace SUDOKU.Puzzle.Component { using Data; using SO; }`

```csharp
[SerializeField] private int maxHealth;
```

---

## Unity Logic 규칙

- MonoBehaviour 클래스명과 파일명은 동일하게 유지한다.
- 한 파일에는 하나의 주요 MonoBehaviour만 둔다.
- 가능한 경우 TryGetComponent를 사용한다.
- Update() 사용은 최소화한다.
- 불필요한 singleton 사용을 피한다.

---

## URP 규칙

- URP 호환 Shader와 Material을 사용한다.
- Built-in Render Pipeline 전용 API 사용을 피한다.
- Post Processing 설정 변경은 요청이 있을 때만 한다.
- 모바일 성능을 고려해 복잡한 Shader 사용을 피한다.
- 과도한 Realtime Light 사용을 피한다.

---

## 모바일 최적화 규칙

- GC Allocation을 최소화한다.
- Update() 안에서 LINQ 사용을 피한다.
- Update() 안에서 FindObjectOfType, GameObject.Find를 사용하지 않는다.
- 자주 생성/삭제되는 오브젝트는 Object Pooling을 고려한다.
- Texture, Audio, Particle 사용 시 메모리 사용량을 고려한다.
- 비동기 로딩이 필요한 경우 Addressables 또는 Scene Loading 구조를 고려한다.
- 과도한 Instantiate/Destroy 반복을 피한다.

---

## UI 규칙

- Legacy Text 대신 TextMeshPro를 사용한다.
- Localization Key는 영문 대문자로 작성하고 단어 및 계층 구분에는 공백 대신 점(`.`)을 사용한다. 예: `SETTINGS.GENERIC.LANGUAGE.TITLE`
- 방향키, `ENTER`, `ESC` 같은 고정 입력 표기는 Localization Binding을 사용하지 않는다.
- 모바일 해상도 대응을 고려한다.
- Canvas rebuild가 과도하게 발생하지 않도록 한다.
- Safe Area 대응이 필요한 UI는 별도 고려한다.
- 버튼/터치 입력은 모바일 조작 기준으로 설계한다.

---

## ScriptableObject 규칙

- 공통 설정값은 ScriptableObject 사용을 우선 고려한다.
- 런타임 상태값을 ScriptableObject 에디터 에셋에 직접 저장하지 않는다.
- 데이터와 로직을 과도하게 섞지 않는다.

---

## 작업 완료 전 확인할 것

- 모듈 단위로 구성 된 작업은 검증을 하기 전 사용자에게 물어보고 검증을 진행한다.
- C# 컴파일 오류가 없어야 한다.
- 누락된 namespace가 없어야 한다.
- Inspector serialized reference가 깨지지 않아야 한다.
- 모바일 성능에 불리한 코드가 없는지 확인한다.
- 변경한 파일 목록을 요약한다.

---

## Codex 작업 방식

- 작은 단위로 수정한다.
- 관련 없는 파일은 수정하지 않는다.
- 수정 후 변경 내용을 요약한다.
- 위험한 변경사항이 있으면 명확히 표시한다.
- 확신이 없는 Unity 설정은 임의로 변경하지 않는다.
- public, private, protected, internal 등 접근 수준과 관계없이 작성한 함수에는 XML 또는 일반 주석을 작성한다.
- 함수 주석에는 함수의 용도와 매개변수의 의미를 정리한다.
- `=>` 문법으로 작성한 expression-bodied member나 한 줄 구현으로 작성한 짧은 함수도 함수라면 용도와 매개변수 주석을 작성한다.
- `Start`, `Update`, `FixedUpdate` 같은 Unity 기본 이벤트 함수와 부모 클래스 함수를 `override`하는 함수는 주석을 생략한다.
- `Start`, `Update`, `FixedUpdate`, `OnEnable`, `OnDisable`, `OnDestroy` 같은 Unity 기본 이벤트 함수는 `#region Unity Callbacks`로 묶어 정리한다.
- Inspector 노출 변수와 중요한 상태 변수에는 용도를 설명하는 주석을 작성한다.
- 코드 주석과 Inspector Tooltip은 한글로 작성한다.
- 함수 외부의 자명한 코드에는 불필요한 주석을 작성하지 않는다.
