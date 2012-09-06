namespace SimpleOwinAspNetHost.Middlewares
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.WebSockets;
    using System.Reflection;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class AspNetWebSocketMiddleware
    {
        private readonly Func<object, bool> _isWebSocketRequest;
        private readonly Action<object, object[]> _acceptWebSocketRequest;
        private readonly Func<object, WebSocket> _getWebSocketFromWebSocketContext;

        public AspNetWebSocketMiddleware(bool autoDetect = true)
        {
            if (autoDetect)
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT ||
                    Environment.OSVersion.Version < new Version(6, 2))
                {
                    return;
                }
            }

            var systemWebDll = AppDomain.CurrentDomain.GetAssemblies()
                   .FirstOrDefault(assembly => assembly.FullName.StartsWith("System.Web,"));

            if (systemWebDll == null)
                return;

            var httpContextBaseType = systemWebDll.GetExportedTypes().FirstOrDefault(type => type.Name == "HttpContextBase");
            if (httpContextBaseType == null)
                return;

            var isWebSocketRequestPropertyInfo = httpContextBaseType.GetProperties()
                .FirstOrDefault(property => property.Name == "IsWebSocketRequest" && property.CanRead);

            if (isWebSocketRequestPropertyInfo == null)
                return;

            _isWebSocketRequest = GetGetMethodByExpression<bool>(isWebSocketRequestPropertyInfo);
            if (_isWebSocketRequest == null)
                return;

            var acceptWebSocketRequestMethodInfo = httpContextBaseType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(methodInfo => methodInfo.Name == "AcceptWebSocketRequest");

            if (acceptWebSocketRequestMethodInfo == null)
                return;

            _acceptWebSocketRequest = Create(acceptWebSocketRequestMethodInfo);
            if (_acceptWebSocketRequest == null)
                return;

            var aspnetWebSocketContextType =
                systemWebDll.GetExportedTypes().FirstOrDefault(type => type.Name == "AspNetWebSocketContext");
            if (aspnetWebSocketContextType == null)
                return;

            var getWebSocketFromWebSocketContextPropertyInfo = aspnetWebSocketContextType.GetProperties()
                .FirstOrDefault(property => property.Name == "WebSocket" && property.CanRead);

            if (getWebSocketFromWebSocketContextPropertyInfo == null)
                return;

            _getWebSocketFromWebSocketContext = GetGetMethodByExpression<WebSocket>(getWebSocketFromWebSocketContextPropertyInfo);
        }

        public bool SupportsWebSockets
        {
            get { return _getWebSocketFromWebSocketContext != null; }
        }

        private static Action<object, object[]> Create(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              Expression.Convert(instanceParameter, method.DeclaringType),
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<Action<object, object[]>> lambda = Expression.Lambda<Action<object, object[]>>(
              call,
              instanceParameter,
              argumentsParameter);

            return lambda.Compile();
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) =>
              Expression.Convert(
                Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).ToArray();
        }

        private static Func<object, T> GetGetMethodByExpression<T>(PropertyInfo propertyInfo)
        {
            MethodInfo getMethodInfo = propertyInfo.GetGetMethod();
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            UnaryExpression instanceCast = (!propertyInfo.DeclaringType.IsValueType) ? Expression.TypeAs(instance, propertyInfo.DeclaringType) : Expression.Convert(instance, propertyInfo.DeclaringType);
            Func<object, object> compiled = Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(instanceCast, getMethodInfo), typeof(object)), instance).Compile();
            return source => (T)compiled(source);
        }

        public static Func<AppFunc, AppFunc> Middleware(bool? force = null)
        {
            return app =>
                env =>
                {
                    return app(env);
                };
        }
    }
}