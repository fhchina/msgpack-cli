﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:2.0.50727.8009
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace MsgPack.Serialization.GeneratedSerializers.ArrayBased {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("MsgPack.Serialization.CodeDomSerializers.CodeDomSerializerBuilder", "0.6.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    public class MsgPack_Serialization_PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructorSerializer : MsgPack.Serialization.MessagePackSerializer<MsgPack.Serialization.PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructor> {
        
        private MsgPack.Serialization.MessagePackSerializer<int> _serializer0;
        
        public MsgPack_Serialization_PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructorSerializer(MsgPack.Serialization.SerializationContext context) : 
                base(context) {
            MsgPack.Serialization.PolymorphismSchema schema0 = default(MsgPack.Serialization.PolymorphismSchema);
            schema0 = null;
            this._serializer0 = context.GetSerializer<int>(schema0);
        }
        
        protected internal override void PackToCore(MsgPack.Packer packer, MsgPack.Serialization.PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructor objectTree) {
            packer.PackArrayHeader(1);
            this._serializer0.PackTo(packer, objectTree.Primitive);
        }
        
        protected internal override MsgPack.Serialization.PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructor UnpackFromCore(MsgPack.Unpacker unpacker) {
            MsgPack.Serialization.PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructor result = default(MsgPack.Serialization.PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructor);
            if (unpacker.IsArrayHeader) {
                int unpacked = default(int);
                int itemsCount = default(int);
                itemsCount = MsgPack.Serialization.UnpackHelpers.GetItemsCount(unpacker);
                int ctorArg0 = default(int);
                ctorArg0 = 0;
                System.Nullable<int> nullable = default(System.Nullable<int>);
                if ((unpacked < itemsCount)) {
                    nullable = MsgPack.Serialization.UnpackHelpers.UnpackNullableInt32Value(unpacker, typeof(MsgPack.Serialization.PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructor), "Int32 Primitive");
                }
                if (nullable.HasValue) {
                    ctorArg0 = nullable.Value;
                }
                unpacked = (unpacked + 1);
                result = new MsgPack.Serialization.PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructor(ctorArg0);
            }
            else {
                int itemsCount0 = default(int);
                itemsCount0 = MsgPack.Serialization.UnpackHelpers.GetItemsCount(unpacker);
                int ctorArg00 = default(int);
                ctorArg00 = 0;
                for (int i = 0; (i < itemsCount0); i = (i + 1)) {
                    string key = default(string);
                    string nullable0 = default(string);
                    nullable0 = MsgPack.Serialization.UnpackHelpers.UnpackStringValue(unpacker, typeof(MsgPack.Serialization.PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructor), "MemberName");
                    if (((nullable0 == null) 
                                == false)) {
                        key = nullable0;
                    }
                    else {
                        throw MsgPack.Serialization.SerializationExceptions.NewNullIsProhibited("MemberName");
                    }
                    if ((key == "Primitive")) {
                        System.Nullable<int> nullable1 = default(System.Nullable<int>);
                        nullable1 = MsgPack.Serialization.UnpackHelpers.UnpackNullableInt32Value(unpacker, typeof(MsgPack.Serialization.PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructor), "Int32 Primitive");
                        if (nullable1.HasValue) {
                            ctorArg00 = nullable1.Value;
                        }
                    }
                    else {
                        unpacker.Skip();
                    }
                }
                result = new MsgPack.Serialization.PolymorphicMemberTypeKnownType_Normal_PrimitiveGetOnlyPropertyAndConstructor(ctorArg00);
            }
            return result;
        }
        
        private static T @__Conditional<T>(bool condition, T whenTrue, T whenFalse)
         {
            if (condition) {
                return whenTrue;
            }
            else {
                return whenFalse;
            }
        }
    }
}