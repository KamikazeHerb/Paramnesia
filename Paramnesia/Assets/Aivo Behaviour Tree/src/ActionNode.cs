using System;
using Test;

namespace AivoTree
{
    public class ActionNode<T> : TreeNode<T>
    {
        private readonly Func<long, T, AivoTreeStatus> _fn;

        public ActionNode(Func<long, T, AivoTreeStatus> p1)
        {
            _fn = p1;
        }

        public ActionNode(Func<long, T, AivoTreeStatus> fn, TreeNode<BehaviourTree> treeNode)
        {
            _fn = fn;
        }
        
        public AivoTreeStatus Tick(long timeTick, T context)
        {
            return _fn(timeTick, context);
        }
    }
}