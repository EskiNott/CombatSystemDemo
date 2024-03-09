using System.Collections.Generic;
using UnityEngine;

namespace EskiNottToolKit
{
    public class MathLibrary
    {
        /// <summary>
        /// 成功概率计算，参数均为小数
        /// </summary>
        /// <param name="Target">成功目标概率</param>
        /// <returns>成功返回True，否则返回False</returns>
        public static bool ProbabilityCalculate(float Target)
        {
            float _rd = UnityEngine.Random.Range(0f, 1f);
            return _rd <= Target;
        }

        /// <summary>
        /// 将List乱序
        /// </summary>
        /// <typeparam name="T">List类型</typeparam>
        /// <param name="originList">原List</param>
        /// <returns>乱序后的一个新List</returns>
        public static List<T> ShuffleList<T>(List<T> originList)
        {
            List<T> _list = new List<T>();
            foreach (var item in originList)
            {
                _list.Insert(UnityEngine.Random.Range(0, _list.Count), item);
            }
            return _list;
        }

        /// <summary>
        /// 得到子对象Transform
        /// </summary>
        /// <param name="parent">父对象</param>
        /// <param name="objectName">对象名</param>
        /// <returns>子对象Transform</returns>
        public static Transform GetChildTrans(Transform parent, string objectName)
        {
            foreach(Transform c in parent)
            {
                if(c.gameObject.name == objectName)
                {
                    return c;
                }
            }
            return null;
        }

        /// <summary>
        /// 迭代查询子Transform是否有某个父Transform
        /// </summary>
        /// <param name="StartPoint">子Transform</param>
        /// <param name="MaxDepthTransform">最大查询深度</param>
        /// <param name="TargetTransform">目标父Transform</param>
        /// <returns></returns>
        public static bool IsChildHasParent(Transform StartPoint, Transform MaxDepthTransform, Transform TargetTransform)
        {
            bool found = false;
            Transform test = StartPoint;
            if (test == TargetTransform)
            {
                found = true;
            }
            else
            {
                while (test != null && MaxDepthTransform != null && test != MaxDepthTransform)
                {
                    if (test == TargetTransform)
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                        test = test.parent;
                    }
                }
            }
            return found;
        }

        /// <summary>
        /// 从相机到鼠标位置发射一条PhysicalRay
        /// </summary>
        /// <returns>RaycastHit 射线数据</returns>
        public static RaycastHit RaycastPhysical()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast((Ray)ray, out hit);
            return hit;
        }
    }
}