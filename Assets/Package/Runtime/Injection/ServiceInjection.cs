using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FinalClick.Services.Injection
{
    public static class ServiceInjection
    {
        public static void Inject(ServiceCollection services, object injectInto)
        {
            IEnumerable<PropertyInfo> propertiesWithInjectAttribute = InjectionPropertyProvider.GetInjectableProperties(injectInto);

            foreach (var prop in propertiesWithInjectAttribute)
            {
                InjectServiceIntoProperty(services, injectInto, prop);
            }
        }

        private static void InjectServiceIntoProperty(ServiceCollection services, object injectInto, PropertyInfo propertyInfo)
        {
            if (services.TryGet(propertyInfo.PropertyType, out object service) == false)
            {
                if (IsNullable(propertyInfo.PropertyType))
                {
                    // Skip injection; leave the property as null
                    return;
                }

                throw new ArgumentException($"Unable to find service of type '{propertyInfo.PropertyType}' for property '{propertyInfo.Name}' on '{injectInto}'");
            }

            if (propertyInfo.CanWrite == false)
            {
                InjectServiceOnPropertyBackingField(injectInto, service, propertyInfo);
                return;
            }

            propertyInfo.SetValue(injectInto, service);
        }

        private static bool IsNullable(Type type)
        {
            // For reference types (including interfaces), they are always nullable
            if (!type.IsValueType)
            {
                return true;
            }

            // For Nullable<T> value types
            return Nullable.GetUnderlyingType(type) != null;
        }
        
        private static void InjectServiceOnPropertyBackingField<T>(object injectInto, T value, PropertyInfo propertyInfo)
        {
            FieldInfo backingField = InjectionPropertyProvider.GetBackingField(injectInto, propertyInfo);

            try
            {
                // ReSharper disable once HeapView.PossibleBoxingAllocation
                backingField.SetValue(injectInto, value);
            }
            catch (Exception)
            {
                Debug.LogError($"Unable to inject '{propertyInfo}' into '{injectInto}'");
                throw;
            }
        }
    }
}