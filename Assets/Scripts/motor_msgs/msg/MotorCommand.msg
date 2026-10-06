int32 id
int32 incremental_point # 位置制御：増加ポイント
int32 target_velocity # 目標速度: rpm
uint8 mode #0: 位置制御， 1: 速度制御
float32 value #目標角度[deg]，目標速度[rpm] 旧epos manager