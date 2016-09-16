using System;
using System.Collections.Generic;
using System.Linq;
using AndromedaCore.Infrastructure;

namespace AndromedaCore
{
    public class ActionFactory
    {
        private static IoCContainer _ioc;

        public ActionFactory(IoCContainer container)
        {
            _ioc = container;
        }

        public static Action InstantiateAction(Type type)
        {
            var constructorInfo = type.GetConstructors().First();
            var paramsInfo = constructorInfo.GetParameters().ToList();
            var resolvedParams = new List<object>();

            foreach (var param in paramsInfo)
            {
                var t = param.ParameterType;
                var res = _ioc.Resolve(t);
                resolvedParams.Add(res);
            }

            return constructorInfo.Invoke(resolvedParams.ToArray()) as Action;
        }

        /// <summary>
        /// Instantiates all actions in a provided list.
        /// </summary>
        /// <param name="typeList"></param>
        /// <returns>List/<Action/></returns>
        public static List<Action> InstantiateAction(IEnumerable<Type> typeList)
        {
            var actionImportList = new List<Action>();

            foreach (var type in typeList)
            {
                var action = InstantiateAction(type);

                if (action == null) { continue; }

                actionImportList.Add(action);
            }

            return actionImportList;
        }
    }
}