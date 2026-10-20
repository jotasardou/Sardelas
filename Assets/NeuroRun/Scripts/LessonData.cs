using System;

namespace NeuroRun
{
    // Conteúdo separado da lógica: para adicionar uma pergunta, basta editar esta lista.
    [Serializable]
    public class Question
    {
        public string prompt, explanation;
        public string[] answers;
        public int correct;
        public Question(string prompt, string a, string b, string c, int correct, string explanation)
        {
            this.prompt = prompt; answers = new[] { a, b, c };
            this.correct = correct; this.explanation = explanation;
        }
    }

    public class Lesson
    {
        public string title, subtitle, briefing;
        public Question[] questions;
        public Lesson(string title, string subtitle, string briefing, params Question[] questions)
        { this.title = title; this.subtitle = subtitle; this.briefing = briefing; this.questions = questions; }
    }

    public static class LessonData
    {
        public static readonly Lesson[] All = {
            new Lesson("Dados e decisões", "FUNDAMENTOS", "Modelos aprendem padrões nos dados. Classificar é escolher uma categoria; regredir é prever um número. Rótulos são as respostas usadas no aprendizado supervisionado.",
                new Question("Um filtro aprende com e-mails marcados como spam ou não spam. Qual é o tipo de aprendizado?", "Supervisionado", "Não supervisionado", "Por reforço", 0,
                    "Supervisionado: os exemplos incluem a resposta desejada, chamada rótulo. O modelo aprende a relacionar as características do e-mail à categoria."),
                new Question("Prever o preço de uma casa é um problema de...", "Agrupamento", "Regressão", "Classificação", 1,
                    "Regressão prevê um valor numérico, como o preço. Classificação prevê uma categoria, como casa ou apartamento."),
                new Question("Quando faz sentido usar Machine Learning?", "Quando precisamos eliminar qualquer erro", "Para substituir toda regra simples", "Quando exemplos ajudam a descobrir padrões difíceis de escrever como regras", 2,
                    "ML é útil quando há exemplos relevantes e padrões difíceis de programar manualmente. Ele não garante acerto e nem sempre é melhor que uma regra simples.")),
            new Lesson("Ideias que conectam", "HISTÓRIA E PARADIGMAS", "Turing discutiu inteligência de máquinas em 1950. Rosenblatt apresentou o perceptron na década de 1950. O conexionismo usa redes de unidades; sistemas simbólicos usam regras e símbolos.",
                new Question("Quem propôs o jogo da imitação em seu artigo de 1950?", "Alan Turing", "Frank Rosenblatt", "Tim Berners-Lee", 0,
                    "Alan Turing propôs uma conversa mediada por texto para discutir inteligência de máquinas. Esse teste não demonstra, por si só, consciência ou compreensão."),
                new Question("Qual modelo está associado ao trabalho de Frank Rosenblatt?", "Árvore de decisão", "Perceptron", "Transformer", 1,
                    "Rosenblatt desenvolveu o perceptron, um classificador com pesos ajustáveis. Um único perceptron tem limitações: não representa qualquer fronteira de decisão."),
                new Question("Qual alternativa descreve o paradigma conexionista?", "Somente regras escritas à mão", "Uma lista de respostas memorizadas", "Redes de unidades conectadas com pesos ajustáveis", 2,
                    "O conexionismo inspira redes neurais. A IA simbólica enfatiza regras e símbolos explícitos. Há sistemas que combinam essas abordagens.")),
            new Lesson("Do dado ao código", "PYTHON E BIBLIOTECAS", "NumPy trabalha com arrays numéricos. No scikit-learn, fit aprende com os dados e predict aplica o modelo. Separe treino e teste antes de ajustar o modelo ou seu pré-processamento.",
                new Question("Qual biblioteca é conhecida por arrays e operações numéricas em Python?", "NumPy", "Flask", "Beautiful Soup", 0,
                    "NumPy oferece arrays e operações numéricas. Flask é voltado a aplicações web e Beautiful Soup à leitura de HTML/XML."),
                new Question("No scikit-learn, o que faz model.fit(X_train, y_train)?", "Desenha um gráfico", "Ajusta o modelo usando exemplos e rótulos de treino", "Mede automaticamente o desempenho no teste", 1,
                    "fit ajusta o modelo. Depois, predict(X_test) produz previsões. Para avaliar, comparamos essas previsões com y_test usando uma métrica adequada."),
                new Question("Qual sequência evita que o teste influencie o treinamento?", "Normalizar tudo, depois separar", "Treinar usando os rótulos do teste", "Separar dados; ajustar no treino; avaliar no teste", 2,
                    "O teste simula dados ainda não vistos. Inclusive médias e escalas do pré-processamento devem ser aprendidas no treino, não no conjunto completo.")),
            new Lesson("A matemática por trás", "NÚMEROS E OTIMIZAÇÃO", "A média soma valores e divide pela quantidade. A descida do gradiente ajusta parâmetros para reduzir uma perda. Probabilidades expressam incerteza, não garantias.",
                new Question("Qual é a média dos valores 2, 4 e 6?", "4", "6", "12", 0,
                    "(2 + 4 + 6) / 3 = 4. A média é uma medida simples de centro e pode ser afetada por valores extremos."),
                new Question("Na descida do gradiente, um pequeno passo costuma seguir...", "A direção de maior aumento da perda", "A direção oposta ao gradiente da perda", "Sempre a direção de pesos maiores", 1,
                    "O gradiente indica a direção de maior crescimento local. Subtraímos um passo proporcional a ele. Uma taxa de aprendizado muito alta pode causar instabilidade."),
                new Question("Um modelo estima 80% de chance de chuva. Isso significa que...", "Vai chover com certeza", "Vai chover durante 80% do dia", "Há incerteza; a previsão precisa ser avaliada em muitos casos", 2,
                    "80% não é garantia nem duração da chuva. Em previsões bem calibradas, eventos anunciados com 80% acontecem aproximadamente 80% das vezes.")),
            new Lesson("Além da memorização", "GENERALIZAÇÃO", "Um modelo deve funcionar em exemplos novos. Overfitting é ajustar demais ao treino. Vazamento de dados contamina a avaliação. A melhor métrica depende do problema.",
                new Question("Um modelo acerta 99% no treino e 60% em dados novos. Qual problema isso sugere?", "Overfitting", "Generalização perfeita", "Ausência de treinamento", 0,
                    "A diferença pode indicar overfitting: o modelo se ajustou a detalhes do treino que não generalizam. Também precisamos investigar diferenças entre os conjuntos."),
                new Question("99 de 100 casos são negativos. Prever sempre negativo dá 99% de acurácia. O modelo é necessariamente útil?", "Sim, pois 99% é sempre excelente", "Não: ele pode perder todos os casos positivos", "Sim, se o conjunto for grande", 1,
                    "Acurácia pode enganar em dados desbalanceados. Se detectar positivos é importante, examine recall, precisão e a matriz de confusão."),
                new Question("Você usa a resposta do exame futuro para prever o diagnóstico atual. O que há de errado?", "O modelo tem poucos parâmetros", "É apenas uma mudança de biblioteca", "Há vazamento: a informação não estaria disponível na previsão real", 2,
                    "Vazamento ocorre quando o treino ou a avaliação usam informação indisponível na situação real. Resultados aparentemente ótimos podem falhar em produção.")),
            new Lesson("Inteligência com critério", "RESPONSABILIDADE", "Dados podem carregar vieses. Correlação não prova causa. No aprendizado por reforço, um agente aprende com recompensas; o objetivo precisa representar o comportamento desejado.",
                new Question("Um sistema de seleção treinado em decisões históricas injustas pode...", "Reproduzir ou ampliar esses vieses", "Eliminar automaticamente toda injustiça", "Ser justo apenas por usar matemática", 0,
                    "Modelos aprendem com dados e objetivos escolhidos por pessoas. É necessário avaliar diferentes grupos, revisar dados e considerar o contexto de uso."),
                new Question("Vendas de sorvete e afogamentos aumentam juntos. Isso prova que sorvete causa afogamentos?", "Sim, toda correlação é causal", "Não; calor e outras variáveis podem explicar a relação", "Sim, desde que existam muitos dados", 1,
                    "Correlação não prova causalidade. Temperatura e maior frequência de banhos podem influenciar as duas variáveis. Contexto e métodos causais importam."),
                new Question("Um agente aprende ações por tentativa, recebendo recompensas. Qual é o paradigma?", "Regressão supervisionada", "Agrupamento", "Aprendizado por reforço", 2,
                    "No reforço, o agente busca recompensa acumulada ao interagir com um ambiente. Uma recompensa mal definida pode incentivar comportamentos indesejados."))
        };
    }
}
