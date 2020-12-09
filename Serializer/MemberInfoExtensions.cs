using System;
using System.Reflection;

namespace Serializer
{
    static class MemberInfoExtension
    {


        public static void SetValue(this MemberInfo member, object obj, object value)
        {
            if (member.MemberType == MemberTypes.Property)
                ((PropertyInfo)member).SetValue(obj, value, null);
            else if (member.MemberType == MemberTypes.Field)
                ((FieldInfo)member).SetValue(obj, value);
            else
                throw new Exception("Property must be of type FieldInfo or PropertyInfo");
        }

        public static object GetValue(this MemberInfo member, object obj)
        {
            if (member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)member).GetValue(obj, null);
            else if (member.MemberType == MemberTypes.Field)
                return ((FieldInfo)member).GetValue(obj);
            else
                throw new Exception("Property must be of type FieldInfo or PropertyInfo");
        }


      
        public static Type GetMemberType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }
    }
}