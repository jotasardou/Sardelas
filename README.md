# NeuroRun

**Aprenda. Desvie. Evolua.** Um jogo 2D de ação e perguntas sobre Machine Learning, desenvolvido em Unity para o Projeto Introdução ao Unity do Insper.

**Aluno:** João Pedro Sardou  
**Código:** https://github.com/jotasardou/Sardelas  
**Perfil itch.io:** https://jotasardou.itch.io/  
**Game Jam:** https://itch.io/jam/projeto-intro-unity-insper-20262

## Como jogar

Você é o agente N-01. Recupere o conhecimento de seis setores de um laboratório digital:

1. Colete os três dados dourados de cada arena.
2. Desvie dos bugs cor-de-rosa. Use o dash para escapar.
3. Entre no núcleo central quando ele ficar verde.
4. Resolva três desafios para desbloquear o próximo setor.

Cada setor começa com **70 segundos e três vidas**. Coletar um dado acrescenta **3 segundos e 25 pontos**. Respostas erradas custam **8 segundos** e mostram uma explicação antes da nova tentativa. A leitura e as pausas não gastam tempo. Acertar de primeira vale 120 pontos; corrigir uma resposta vale 70. O tempo que sobra no setor vale dois pontos por segundo inteiro.

Ao perder, você pode reiniciar o setor atual ou voltar ao menu. Reiniciar restaura vidas e dados e retorna o score ao início do setor; o tempo total de exploração conserva as tentativas anteriores. O recorde é salvo localmente no navegador depois de uma vitória. Não há segredos obrigatórios nem ações irreversíveis que impeçam completar a partida.

| Ação | Teclado e mouse | Controle Xbox / compatível |
| --- | --- | --- |
| Mover | WASD ou setas | Analógico esquerdo ou direcional |
| Dash | Espaço ou Shift esquerdo | B |
| Pausar / retomar | Esc | Start |
| Escolher resposta | 1, 2, 3 ou clique | Direcional e A |
| Navegar nos menus | Mouse ou Tab e Enter | Direcional e A |
| Som | Botão SOM / MUDO | Navegação do menu quando disponível; botão também aceita mouse |

**Plataforma:** navegador desktop, WebGL. Celulares e controles físicos específicos não foram validados. O controle foi exercitado com um dispositivo virtual do Unity Input System; faça um teste com o controle real antes da apresentação.

## Conteúdo dos setores

| Setor | O que ensina |
| --- | --- |
| Dados e decisões | Aprendizado supervisionado, regressão e motivações para ML |
| Ideias que conectam | Alan Turing, Frank Rosenblatt e conexionismo |
| Do dado ao código | NumPy, scikit-learn, `fit` e separação de treino/teste |
| A matemática por trás | Média, gradiente e interpretação de probabilidades |
| Além da memorização | Overfitting, acurácia em dados desbalanceados e vazamento |
| Inteligência com critério | Viés, correlação/causalidade e aprendizado por reforço |

O jogo ensina conceitos introdutórios; não treina um modelo real nem executa Python. As explicações são parte da mecânica, e o guia de campo permite revisar as ideias antes da missão.

## Abrir na Unity

- Versão usada: **Unity 6000.6.6f1**, template Universal 2D.
- Adicione esta pasta ao Unity Hub e abra `Assets/NeuroRun/Scenes/NeuroRun.unity`.
- Pressione Play. O menu inicial abre automaticamente.
- A cena `Assets/Scenes/SampleScene.unity` conserva o trabalho inicial do tutorial.
- Os prefabs de jogador, inimigo e dado estão em `Assets/NeuroRun/Prefabs`.

O jogo usa Rigidbody2D, colliders, triggers, prefabs, scripts MonoBehaviour, Canvas, áudio e Input System. A arena e a UI são montadas em C# no início da partida para reutilizar o mesmo layout entre os seis setores. O menu `NeuroRun > Criar cena do jogo` é uma ferramenta opcional de reconstrução: use-o apenas quando quiser recriar a cena a partir do código.

## Organização do código

| Arquivo | Responsabilidade |
| --- | --- |
| `NeuroRunGame.cs` | Estados, tempo, score, vida, perguntas e progressão |
| `RunnerController.cs` | Movimentação, dash, invulnerabilidade e colisões |
| `BugEnemy.cs` | Patrulha e perseguição variável |
| `DataPickup.cs` | Coleta por trigger, com proteção contra contagem duplicada |
| `ArenaFactory.cs` | Paredes e distribuição dos prefabs em cada arena |
| `LessonData.cs` | 18 perguntas, alternativas e explicações |
| `NeuroUI.cs` | Menu, HUD, guia, perguntas e telas de resultado |
| `NeuroAudio.cs` | Música, efeitos e opção de silenciar |
| `Editor/NeuroRunBuilder.cs` | Geração de sprites, prefabs, cena e configuração WebGL |
| `Tools/generate_audio.py` | Geração original de música e efeitos WAV |
| `Tools/ValidateGame.cs` | Teste de integração, executado somente no Editor via CLI |

Os scripts têm comentários em português e usam estados explícitos para bloquear movimento durante menus e leitura. O personagem e os inimigos se movem em `FixedUpdate`, enquanto UI e cronômetros são atualizados em `Update`.

## Build conforme o vídeo da disciplina

Referência: [Unity WebGL Build — 1000FPSStudio](https://www.youtube.com/watch?v=BzUWHu_q84A).

1. Instalar **WebGL Build Support** pelo Unity Hub (00:08–00:54).
2. Em **File > Build Profiles**, selecionar **Web** e **Switch Platform** (01:04–01:24).
3. Em **Player Settings > Publishing Settings**, usar **Compression Format: Disabled**. O vídeo também aceita Gzip; esta entrega usa Disabled, como a demonstração (01:38–01:58).
4. Manter **960 × 600** e **WebGL Template: Default** (01:59–02:07).
5. Incluir `Assets/NeuroRun/Scenes/NeuroRun.unity` como cena habilitada do build.
6. Executar **Build and Run**, salvando em `Builds/WebGL`, para testar pelo servidor local (02:09–02:45).
7. Compactar todo o conteúdo gerado, incluindo `index.html`, `Build/` e `TemplateData/` (02:51–03:11). No ZIP, `index.html` deve ficar na raiz, conforme as exigências do itch.io.

A compilação não é versionada no Git, por ser um artefato gerado. O ZIP de entrega fica em `Delivery/NeuroRun-WebGL.zip` após a compilação. Não abra `index.html` por `file://`; use Build and Run ou um servidor HTTP.

## Rubrica: onde cada item aparece

| Item | Evidência no jogo/projeto |
| --- | --- |
| Tutorial | Cena inicial preservada; física 2D, movimento, coleta, prefabs, áudio, menu e fim de partida na entrega |
| Tempo | Contagem regressiva, bônus por coleta, penalidade por erro, derrota por tempo e tempo final |
| Inimigos | Bugs em patrulha; a partir do segundo setor alternam perseguição; contato tira vida |
| Visual | Sprites próprios, paleta coerente, bot, bugs, dados e núcleo distinguíveis por forma e cor |
| Áudio | Música original em loop e sete efeitos distintos; botão de som |
| UI | Score, tempo, integridade, dados, recarga do dash, estado da missão e resultado |
| Controle | Input System, teclado/mouse e gamepad/Xbox |
| Level design | Tema ML, seis layouts progressivos e 18 desafios |
| Menu e recuperação | Menu, guia, pausa, reinício de setor e retorno ao menu após vitória/derrota |
| Publicação | WebGL; checklist e texto para itch.io em `Delivery/` |

Os testes de integração verificam colisão, movimento por gamepad virtual, coleta real por trigger, pausa, derrota por tempo/vida, penalidade de resposta errada, reinício e vitória pelos seis setores. O relatório está em `Delivery/validation.txt`.

## Créditos e referências

- **Concepção e projeto acadêmico:** João Pedro Sardou.
- **Assistência de programação, conteúdo, arte procedural e documentação:** OpenAI Codex, durante o desenvolvimento deste projeto. O histórico do repositório registra a implementação.
- **Base didática:** [Tutorial Introdução Rápida, disciplina Jogos Digitais — Insper](https://slender-ceder-1da.notion.site/Tutorial-Introdu-o-R-pida-1a6d1ae1773780a7a59cf823a8b599ec?pvs=4). A implementação de NeuroRun usa esses conceitos, com scripts próprios.
- **Sprites, capa e UI:** formas geométricas originais criadas para este projeto. Os sprites são gerados por `NeuroRunBuilder.cs`.
- **Música e efeitos do jogo final:** síntese original por `Tools/generate_audio.py`, sem gravações externas.
- **Fonte DejaVu Sans:** projeto DejaVu / Bitstream Vera. Licença incluída em `Assets/NeuroRun/Fonts/LICENSE-DejaVu.txt`.
- **Unity:** engine, template Universal 2D e pacotes oficiais indicados em `Packages/manifest.json`; recursos do template conservam suas licenças.
- **Áudio preexistente do tutorial:** [Coin, por chieuk no Pixabay](https://pixabay.com/sound-effects/film-special-effects-coin-257878/), sob a [Pixabay Content License](https://pixabay.com/service/license-summary/). Conservado na cena original como `chieuk-coin-257878.mp3`; não utilizado no build NeuroRun.

Referências para as explicações de ML:

- [scikit-learn: primeiros passos](https://scikit-learn.org/stable/getting_started.html), [validação e divisão dos dados](https://scikit-learn.org/stable/modules/cross_validation.html).
- [NumPy: guia para iniciantes](https://numpy.org/doc/stable/user/absolute_beginners.html).
- [Alan Turing, Computing Machinery and Intelligence (1950)](https://doi.org/10.1093/mind/LIX.236.433).
- [Cornell: história do perceptron e trabalho de Frank Rosenblatt](https://news.cornell.edu/stories/2019/09/professors-perceptron-paved-way-ai-60-years-too-soon).
- [Sutton e Barto: Reinforcement Learning](http://incompleteideas.net/book/the-book-2nd.html).

## Entrega externa

A rubrica exige **GitHub público, jogo publicado no itch.io, submissão à Game Jam e registro completo no Blackboard**. Ter somente os arquivos locais ou um link de perfil não conclui essas etapas. Consulte `Delivery/ENTREGA.md` para os passos e o status verificado.
