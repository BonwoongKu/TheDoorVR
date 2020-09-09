# InteractBasicAbstract
- 상호작용 오브젝트의 최상위 추상화 클래스
- 공통 데이터 및 기능을 소유

# InteractGrabAbstract
- 잡을 수 있는 오브젝트의 추상화 클래스
- 상속 후 잡았을때의 구체적인 기능을 구현

# InteractPullAbstract
- 잡고 당기거나 밀수 있는 오브젝트의 추상화 클래스
- 'InteractGrabAbstract' 상속 후 기능 구현

# InteractRayAbstract
- HMD에 시선에 의해 상호작용을 하는 추상화 클래스

# InteractiveItem
- 잡아서 들거나 던질 수 있는 오브젝트의 기본 클래스

# InteractiveItemPieceOfMemory
- 'InteractiveItem' 을 상속받아 세부기능을 구현한 아이템 오브젝트
- 기억의조각

# InteractivePieceOfMemoryHole
- 상호작용 오브젝트의 최상위 클래스를 상속받아 'InteractiveItemPieceOfMemory'의 오브젝트와 상호작용
- 기억의조각을 설치할 수 있는 구멍
