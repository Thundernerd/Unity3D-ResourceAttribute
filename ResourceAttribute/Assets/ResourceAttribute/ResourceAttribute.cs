using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using RA;

namespace RA {

    public static class MemberInfoExtensions {

        public static bool CanWrite( this MemberInfo info ) {
            switch ( info.MemberType ) {
                case MemberTypes.Field:
                    return true;
                case MemberTypes.Property:
                    var p = ( info as PropertyInfo );
                    return p.CanWrite;
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                case MemberTypes.Event:
                case MemberTypes.TypeInfo:
                case MemberTypes.Custom:
                case MemberTypes.NestedType:
                case MemberTypes.All:
                default:
                    return false;
            }
        }

        public static Type GetMemberType( this MemberInfo info, bool includeArrays = false ) {
            switch ( info.MemberType ) {
                case MemberTypes.Event:
                    var e = info as EventInfo;
                    return e.EventHandlerType;
                case MemberTypes.Field:
                    var f = info as FieldInfo;
                    return f.FieldType.IsArray && includeArrays ? f.FieldType.GetElementType() : f.FieldType;
                case MemberTypes.Method:
                    var m = info as MethodInfo;
                    return m.ReturnType;
                case MemberTypes.Property:
                    var p = info as PropertyInfo;
                    return p.PropertyType.IsArray && includeArrays ? p.PropertyType.GetElementType() : p.PropertyType;
                case MemberTypes.Constructor:
                case MemberTypes.TypeInfo:
                case MemberTypes.Custom:
                case MemberTypes.NestedType:
                case MemberTypes.All:
                default:
                    return null;
            }
        }

        public static void SetValue( this MemberInfo info, object obj, object value ) {
            switch ( info.MemberType ) {
                case MemberTypes.Field:
                    var f = ( info as FieldInfo );
                    f.SetValue( obj, value );
                    break;
                case MemberTypes.Property:
                    var p = ( info as PropertyInfo );
                    p.SetValue( obj, value, null );
                    break;
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                case MemberTypes.Event:
                case MemberTypes.TypeInfo:
                case MemberTypes.Custom:
                case MemberTypes.NestedType:
                case MemberTypes.All:
                default:
                    break;
            }
        }
    }
}

public static class RAExtensions {

    private static Dictionary<Type, List<MemberInfo>> TypeMembers = new Dictionary<Type, List<MemberInfo>>();

    public static void LoadResources( this MonoBehaviour behaviour ) {
        var bType = behaviour.GetType();
        var rType = typeof( ResourceAttribute );
        var mType = typeof( UnityEngine.Object );
        List<MemberInfo> members;

        if ( TypeMembers.ContainsKey( bType ) ) {
            members = TypeMembers[bType];
        } else {
            members = bType.GetMembers( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                .Where( m =>
                ( m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property )
                && ( m.GetMemberType( true ).IsSubclassOf( mType ) || m.GetMemberType( true ) == mType )
                && m.GetCustomAttributes( rType, false ).Length == 1 ).ToList();

            members.OrderBy( m => m.MemberType ).ThenBy( m => m.Name );
            TypeMembers.Add( bType, members );
        }

        foreach ( var item in members ) {
            var attribute = item.GetCustomAttributes( rType, false )[0] as ResourceAttribute;
            var memberType = item.GetMemberType();
            var elementType = item.GetMemberType( true );

            if ( string.IsNullOrEmpty( attribute.Name ) || attribute.Name.EndsWith( "/" ) ) {
                UnityEngine.Object[] resources;
                var isUObject = elementType == typeof( UnityEngine.Object );


                if ( isUObject ) {
                    resources = Resources.LoadAll( attribute.Name );
                } else {
                    resources = Resources.LoadAll( attribute.Name, elementType );
                }

                if ( resources.Length == 0 ) continue;

                if ( memberType.IsArray ) {
                    if ( isUObject ) {
                        item.SetValue( behaviour, resources );
                    } else {
                        var array = Array.CreateInstance( elementType, resources.Length );
                        for ( int i = 0; i < resources.Length; i++ ) {
                            array.SetValue( resources[i], i );
                        }
                        item.SetValue( behaviour, array );
                    }
                }
            } else {
                UnityEngine.Object resource;

                if ( attribute.ForceType ) {
                    resource = Resources.Load( attribute.Name, memberType );
                } else {
                    resource = Resources.Load( attribute.Name );
                }

                item.SetValue( behaviour, resource );
            }
        }
    }
}

[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
sealed class ResourceAttribute : Attribute {

    public readonly string Name;
    public readonly bool ForceType;

    public ResourceAttribute( string name ) {
        Name = name;
        ForceType = false;
    }

    public ResourceAttribute( string name, bool forceType ) {
        Name = name;
        ForceType = forceType;
    }
}