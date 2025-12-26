using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.DataStructures.Trees
{
    public class QuestionNode
    {
        public Question Data;
        public QuestionNode Left;
        public QuestionNode Right;

        public QuestionNode(Question question)
        {
            Data = question;
        }
    }
}
