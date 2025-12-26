using PersonalizedLearningPath.Models;

namespace PersonalizedLearningPath.DataStructures.Trees
{
    public class BinaryQuestionTree
    {
        private readonly Dictionary<int, Question> _byTreeIndex;

        public BinaryQuestionTree(IEnumerable<Question> questions)
        {
            _byTreeIndex = questions.ToDictionary(q => q.TreeIndex);
        }

        public Question? GetRoot() => GetByTreeIndex(0);

        public Question? GetByTreeIndex(int treeIndex)
        {
            return _byTreeIndex.TryGetValue(treeIndex, out var q) ? q : null;
        }

        // Navigation MUST be based on TreeIndex 'k' for a complete binary tree:
        // left = 2k+1, right = 2k+2.
        public Question? GetNextByTreeIndex(int currentTreeIndex, bool isCorrect)
        {
            int nextTreeIndex = isCorrect
                ? (2 * currentTreeIndex + 2)
                : (2 * currentTreeIndex + 1);

            return GetByTreeIndex(nextTreeIndex);
        }
    }
}

