# FPS 3D — Jogo de Ação em Unity

Jogo FPS 3D completo e jogável criado em **Unity 6000.0.83f1** com C#.
O jogador atravessa um mapa urbano/rural, enfrenta inimigos com IA, passa por
checkpoints e completa a missão.

---

## Como abrir o projeto

1. Instale o **Unity Hub** e a versão **6000.0.83f1**.
2. No Unity Hub → **Open** → selecione a pasta `FPS 3D/FPS 3D`.
3. Aguarde o import dos pacotes e a compilação.

> Na primeira abertura o projeto **edita e salva automaticamente a cena**
> `Assets/Scenes/Level01.unity` (menu `FPS → Build FPS Scene` executa o mesmo
> processo manualmente). Aguarde a mensagem
> `[FPS] Scene built successfully!` no Console.

> **Pacotes**: `com.unity.ugui` e `com.unity.cloud.gltfast` serão baixados
> automaticamente pelo Package Manager. Se o Editor estiver aberto, clique na
> janela do Unity para forçar o download/refresh dos pacotes.

## Como executar

1. Abra a cena `Assets/Scenes/Level01.unity` (duplo clique).
2. Pressione **Play** no topo do Editor.
3. O jogo já começa com a UI (vida, munição, mira) e inimigos ativos.

> **Navegação dos inimigos (NavMesh)**: a malha é gerada automaticamente na
> construção da cena. Se você mover obstáculos, use `FPS → Build FPS Scene`
> novamente para rebake.

## Controles do jogador

| Ação            | Tecla(s)                     |
|-----------------|------------------------------|
| Mover           | `W A S D`                    |
| Correr          | `Shift (esquerdo)` + WASD    |
| Pular           | `Espaço`                     |
| Olhar           | Movimento do mouse           |
| Atirar          | Botão esquerdo do mouse      |
| Arma 1 (Pistola)| `1`                          |
| Arma 2 (Rifle)  | `2`                          |
| Arma 3 (Espingarda) | `3`                      |
| Recarregar      | `R`                          |
| Pausar          | `Esc`                        |
| Reiniciar (Game Over / Missão) | `R`   |

## Estrutura de pastas

```
Assets/
├─ Scripts/
│  ├─ Player/        FpsPlayerController, PlayerDeathHandler, PlayerMovement,
│  │                 PlayerLook, PlayerHealth
│  ├─ Weapons/       WeaponData (SO), WeaponController, WeaponEntry
│  ├─ Enemies/       EnemyAI, EnemyHealth
│  ├─ Systems/       HealthSystem, IDamageable, Checkpoint, GameManager, MusicPlayer
│  └─ UI/            FpsHud, PauseMenu, GameOverMenu, MissionCompleteMenu
├─ Editor/
│  ├─ FpsSceneBuilder.cs     Constrói a cena Level01 (primitivos + wire-up)
│  ├─ FpsCityProps.cs        Insere os modelos reais baixados (Poly Haven)
│  ├─ FpsAudioGenerator.cs   Gera música e efeitos sonoros (WAV)
│  └─ ProjectScaffolder.cs   Cria cenas/builder settings/prefab básico do Player
├─ Scenes/           Level01.unity
├─ Prefabs/          WeaponData_*.asset (pistola, rifle, espingarda)
├─ Materials/        Environment/ (cenário), Characters/ (inimigos), Weapons/
├─ Models/
│  └─ Environment/   Modelos reais CC0 (Poly Haven): postes, bancos, barreiras,
│                    fachada de prédio, postes de energia e rato de rua (.gltf)
├─ Prefabs/
│  ├─ Player/        BasicPlayer.prefab (CharacterController + câmera + scripts)
│  └─ Weapons/       WeaponData_Pistol/Rifle/Shotgun.asset
├─ Scenes/           MainMenu.unity, Level01.unity (principal), Level02..07,
│                    Test/TestScene.unity
├─ Textures/         Environment/, Characters/, Weapons/
└─ UI/               Images/, Fonts/, Icons/
```

## Descrição dos principais scripts

- **FpsPlayerController** — movimento WASD, correr, pular, gravidade, câmera
  com mouse (CharacterController + rotação vertical limitada).
- **WeaponController / WeaponData** — sistema modular de armas: dano, cadência,
  alcance, carregador, munição reserva, tempo de recarga, "pellets"/spread
  (espingarda). Novas armas = novo `WeaponData` + entrada na lista.
- **HealthSystem** — vida genérica reutilizável para Player e inimigos
  (implementa `IDamageable`), eventos `OnDamaged/OnHealed/OnDied`.
- **EnemyAI** — IA em NavMesh com patrulha, detecção por distância + campo de
  visão, perseguição e ataque corpo a corpo; morre e para de agir.
- **FpsHud** — barra e número de vida, munição/reserva, nome da arma, mira,
  mensagens (recarga / sem munição / checkpoint).
- **GameManager** — estado do jogo, pausa, Game Over, restart no último
  checkpoint, conclusão da missão, cursor do mouse.
- **FpsSceneBuilder (Editor)** — cria a cena inteira com referências corretas
  e bake de NavMesh; executado automaticamente na primeira abertura.
- **FpsCityProps (Editor)** — coloca os modelos 3D reais baixados na rua
  (use o menu `FPS → Place Real City Props (Poly Haven)`).

## Como adicionar novas armas

1. Crie um `WeaponData` (Assets → Create → FPS → Weapon Data) ou reutilize o
   menu `FPS → Build FPS Scene` (recria pistola/rifle/espingarda).
2. Atribua `slot` único (1, 2, 3...) — a troca usa as teclas 1–3.
3. Vincule um `WeaponEntry` (data + view + audioSource) no `WeaponController`
   do Player; no script a troca é automática por `GetSlotKey`.

## Como usar seus próprios modelos 3D reais

Coloque arquivos importáveis pelo Unity (`.glb`/`.gltf` com o pacote glTFast,
ou `.fbx`/`.obj`) em `Assets/Environment/Imported/`. O script
`FpsCityProps` carrega os modelos a partir dessa pasta. Para substituir o
visual do Player/inimigos por humanos, basta colocar o modelo como filho do
`Player` (ou da cápsula do inimigo) e ocultar o primitivo padrão.

## Créditos dos assets reais

Modelos da rua/cidade: **Poly Haven** (modelos CC0 — street lamp, bench, road
barrier, electricity poles, urban apartments facade, street rat.
https://polyhaven.com/). Música e efeitos: gerados proceduralmente no projeto.