#region -- License Terms --
//
// MessagePack for CLI
//
// Copyright (C) 2010-2013 FUJIWARA, Yusuke
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
#endregion -- License Terms --

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using MsgPack.Serialization.AbstractSerializers;

namespace MsgPack.Serialization.EmittingSerializers
{
	internal abstract class ILEmittingSerializerBuilder<TContext, TObject> : SerializerBuilder<TContext, ILConstruct, TObject>
		where TContext : ILEmittingContext
	{
		protected ILEmittingSerializerBuilder()
			: this( "ILDynamicMethodHost", new Version() )
		{
		}

		protected ILEmittingSerializerBuilder( string assemblyName, Version version )
			: base( assemblyName, version )
		{
		}

		protected override void EmitMethodPrologue( TContext context, MethodInfo metadata )
		{
			switch ( metadata.Name )
			{
				case "PackToCore":
				{
					context.IL = context.Emitter.GetPackToMethodILGenerator();
					break;
				}
				case "UnpackFromCore":
				{
					context.IL = context.Emitter.GetUnpackFromMethodILGenerator();
					break;
				}
				case "UnpackToCore":
				{
					context.IL = context.Emitter.GetUnpackToMethodILGenerator();
					break;
				}
				default:
				{
					throw new NotSupportedException( metadata.Name );
				}
			}
		}

		protected override void EmitMethodEpilogue( TContext context, MethodInfo metadata, IList<ILConstruct> constructs )
		{
			try
			{
				foreach ( var construct in constructs )
				{
					if ( construct == null )
					{
						continue;
					}

					construct.Evaluate( context.IL );
				}

				context.IL.EmitRet();
			}
			finally
			{
				context.IL.FlushTrace();
			}
		}

		protected override ILConstruct EmitSequentialStatements( TContext context, Type contextType, IEnumerable<ILConstruct> statements )
		{
			return ILConstruct.Sequence( contextType, statements );
		}

		protected override ILConstruct EmitStatementExpression( TContext context, ILConstruct statement, ILConstruct contextExpression )
		{
			return ILConstruct.Composite( statement, contextExpression );
		}

		protected override ILConstruct MakeNullLiteral( TContext context, Type contextType )
		{
			return ILConstruct.Literal( contextType, default( object ), il => il.EmitLdnull() );
		}

		protected override ILConstruct MakeInt32Literal( TContext context, int constant )
		{
			switch ( constant )
			{
				case 0:
				{
					return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4_0() );
				}
				case 1:
				{
					return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4_1() );
				}
				case 2:
				{
					return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4_2() );
				}
				case 3:
				{
					return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4_3() );
				}
				case 4:
				{
					return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4_4() );
				}
				case 5:
				{
					return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4_5() );
				}
				case 6:
				{
					return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4_6() );
				}
				case 7:
				{
					return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4_7() );
				}
				case 8:
				{
					return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4_8() );
				}
				case -1:
				{
					return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4_M1() );
				}
				default:
				{
					// ReSharper disable RedundantIfElseBlock
					if ( 0 <= constant && constant <= 255 )
					{
						return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4_S( unchecked( ( byte )constant ) ) );
					}
					else
					{
						return ILConstruct.Literal( typeof( int ), constant, il => il.EmitLdc_I4( constant ) );
					}
					// ReSharper restore RedundantIfElseBlock
				}
			}
		}

		protected override ILConstruct MakeInt64Literal( TContext context, long constant )
		{
			return ILConstruct.Literal( typeof( long ), constant, il => il.EmitLdc_I8( constant ) );
		}

		protected override ILConstruct MakeStringLiteral( TContext context, string constant )
		{
			return ILConstruct.Literal( typeof( string ), constant, il => il.EmitLdstr( constant ) );
		}

		protected override ILConstruct EmitThisReferenceExpression( TContext context )
		{
			return ILConstruct.Literal( context.SerializerType, "(this)", il => il.EmitLdarg_0() );
		}

		protected override ILConstruct EmitBoxExpression( TContext context, Type valueType, ILConstruct value )
		{
			return
				ILConstruct.UnaryOperator(
					"box",
					value,
					( il, val ) =>
					{
						val.LoadValue( il, false );
						il.EmitBox( valueType );
					}
				);
		}

		protected override ILConstruct EmitNotExpression( TContext context, ILConstruct booleanExpression )
		{
			if ( booleanExpression.ContextType != typeof( bool ) )
			{
				throw new ArgumentException(
					String.Format( CultureInfo.CurrentCulture, "Not expression must be Boolean elementType, but actual is '{0}'.", booleanExpression.ContextType ),
					"booleanExpression"
				);
			}

			return
				ILConstruct.UnaryOperator(
					"!",
					booleanExpression,
					( il, val ) =>
					{
						val.LoadValue( il, false );
						il.EmitNot();
					},
					( il, val, @else ) =>
					{
						val.LoadValue( il, false );
						il.EmitBrtrue( @else );
					}
				);
		}

		protected override ILConstruct EmitEqualsExpression( TContext context, ILConstruct left, ILConstruct right )
		{
			var equality = left.ContextType.GetMethod( "op_Equality" );
			return
				ILConstruct.BinaryOperator(
					"==",
					typeof( bool ),
					left,
					right,
					( il, l, r ) =>
					{
						l.LoadValue( il, false );
						r.LoadValue( il, false );
						if ( equality == null )
						{
							il.EmitCeq();
						}
						else
						{
							il.EmitAnyCall( equality );
						}
					},
					( il, l, r, @else ) =>
					{
						l.LoadValue( il, false );
						r.LoadValue( il, false );
						if ( equality == null )
						{
							il.EmitCeq();
						}
						else
						{
							il.EmitAnyCall( equality );
						}

						il.EmitBrfalse( @else );
					}
				);
		}

		protected override ILConstruct EmitGreaterThanExpression( TContext context, ILConstruct left, ILConstruct right )
		{
#if DEBUG
			Contract.Assert( left.ContextType.IsPrimitive && left.ContextType != typeof( string ) );
#endif
			var greaterThan = left.ContextType.GetMethod( "op_GreaterThan" );
			return
				ILConstruct.BinaryOperator(
					">",
					typeof( bool ),
					left,
					right,
					( il, l, r ) =>
					{
						l.LoadValue( il, false );
						r.LoadValue( il, false );
						if ( greaterThan == null )
						{
							il.EmitCgt();
						}
						else
						{
							il.EmitAnyCall( greaterThan );
						}
					},
					( il, l, r, @else ) =>
					{
						l.LoadValue( il, false );
						r.LoadValue( il, false );
						if ( greaterThan == null )
						{
							il.EmitCgt();
						}
						else
						{
							il.EmitAnyCall( greaterThan );
						}
						il.EmitBrfalse( @else );
					}
				);
		}

		protected override ILConstruct EmitLessThanExpression( TContext context, ILConstruct left, ILConstruct right )
		{
#if DEBUG
			Contract.Assert( left.ContextType.IsPrimitive && left.ContextType != typeof( string ) );
#endif
			var lessThan = left.ContextType.GetMethod( "op_LessThan" );
			return
				ILConstruct.BinaryOperator(
					"<",
					typeof( bool ),
					left,
					right,
					( il, l, r ) =>
					{
						l.LoadValue( il, false );
						r.LoadValue( il, false );
						if ( lessThan == null )
						{
							il.EmitClt();
						}
						else
						{
							il.EmitAnyCall( lessThan );
						}
					},
					( il, l, r, @else ) =>
					{
						l.LoadValue( il, false );
						r.LoadValue( il, false );
						if ( lessThan == null )
						{
							il.EmitClt();
						}
						else
						{
							il.EmitAnyCall( lessThan );
						}

						il.EmitBrfalse( @else );
					}
				);
		}

		protected override ILConstruct EmitIncrementExpression( TContext context, ILConstruct int32Value )
		{
			return
				ILConstruct.UnaryOperator(
					"++",
					int32Value,
					( il, variable ) =>
					{
						variable.LoadValue( il, false );
						il.EmitLdc_I4_1();
						il.EmitAdd();
						variable.StoreValue( il );
					}
				);
		}

		protected override ILConstruct EmitTypeOfExpression( TContext context, Type type )
		{
			return
				ILConstruct.Literal(
					typeof( Type ),
					type,
					il => il.EmitTypeOf( type )
				);
		}

		protected override ILConstruct DeclareLocal( TContext context, Type type, string name )
		{
			return
				ILConstruct.Variable(
					context,
					type,
					name
				);
		}

		protected override ILConstruct EmitInvokeVoidMethod( TContext context, ILConstruct instance, MethodInfo method, params ILConstruct[] arguments )
		{
			return
				method.ReturnType == typeof( void )
					? ILConstruct.Invoke( instance, method, arguments )
					: ILConstruct.Sequence(
						typeof( void ),
						new[]
						{
							ILConstruct.Invoke( instance, method, arguments ),
							ILConstruct.Instruction( "pop", typeof( void ), false, il => il.EmitPop() )
						}
					);
		}

		protected override ILConstruct EmitCreateNewObjectExpression( TContext context, ILConstruct variable, ConstructorInfo constructor, params ILConstruct[] arguments )
		{
			return ILConstruct.NewObject( variable, constructor, arguments );
		}

		protected override ILConstruct EmitCreateNewArrayExpression( TContext context, Type elementType, int length, IEnumerable<ILConstruct> initialElements )
		{
			var array =
				ILConstruct.Variable(
					context,
					elementType.MakeArrayType(),
					"array"
				);

			return
				ILConstruct.Composite( 
					ILConstruct.Sequence( 
						array.ContextType,
						new ILConstruct[]
						{
							array,
							ILConstruct.Instruction( 
								"CreateArray",
								array.ContextType,
								false,
								il =>
								{
									il.EmitNewarr( elementType, length );
									array.StoreValue( il );
									var index = 0;
									foreach ( var initialElement in initialElements )
									{
										array.LoadValue( il, false );
										this.MakeInt32Literal( context, index ).LoadValue( il, false );
										initialElement.LoadValue( il, false );
										il.EmitStelem( elementType );
										index++;
									}
								}
							)
						}
					),
					array
				);
		}

		protected override ILConstruct EmitInvokeMethodExpression( TContext context, ILConstruct instance, MethodInfo method, IEnumerable<ILConstruct> arguments )
		{
			return ILConstruct.Invoke( instance, method, arguments );
		}

		protected override ILConstruct EmitGetPropretyExpression( TContext context, ILConstruct instance, PropertyInfo property )
		{
			return ILConstruct.Invoke( instance, property.GetGetMethod( true ), ILConstruct.NoArguments );
		}

		protected override ILConstruct EmitGetFieldExpression( TContext context, ILConstruct instance, FieldInfo field )
		{
			return ILConstruct.LoadField( instance, field );
		}

		protected override ILConstruct EmitSetProprety( TContext context, ILConstruct instance, PropertyInfo property, ILConstruct value )
		{
#if DEBUG
// ReSharper disable PossibleNullReferenceException
			Contract.Assert(
				property.GetSetMethod( true ) != null,
				property.DeclaringType.FullName + "::" + property.Name + ".set != null" 
			);
// ReSharper restore PossibleNullReferenceException
#endif
			return ILConstruct.Invoke( instance, property.GetSetMethod( true ), new[] { value } );
		}

		protected override ILConstruct EmitSetField( TContext context, ILConstruct instance, FieldInfo field, ILConstruct value )
		{
			return ILConstruct.StoreField( instance, field, value );
		}

		protected override ILConstruct EmitLoadVariableExpression( TContext context, ILConstruct variable )
		{
			return ILConstruct.Instruction( "load", variable.ContextType, false, il => variable.LoadValue( il, false ) );
		}

		protected override ILConstruct EmitStoreVariableStatement( TContext context, ILConstruct variable, ILConstruct value )
		{
			return ILConstruct.StoreLocal( variable, value );
		}

		protected override ILConstruct EmitThrowExpression( TContext context, Type expressionType, ILConstruct exceptionExpression )
		{
			return
				ILConstruct.Instruction(
					"throw",
					expressionType,
					true,
					il =>
					{
						exceptionExpression.LoadValue( il, false );
						il.EmitThrow();
					}
				);
		}

		protected override ILConstruct EmitTryFinallyExpression( TContext context, ILConstruct tryExpression, ILConstruct finallyStatement )
		{
			return
				ILConstruct.Instruction(
					"try-finally",
					tryExpression.ContextType,
					false,
					il =>
					{
						il.BeginExceptionBlock();
						tryExpression.Evaluate( il );
						il.BeginFinallyBlock();
						finallyStatement.Evaluate( il );
						il.EndExceptionBlock();
					}
				);
		}

		protected override ILConstruct EmitConditionalExpression( TContext context, ILConstruct conditionExpression, ILConstruct thenExpression, ILConstruct elseExpression )
		{
			return
				ILConstruct.IfThenElse(
					conditionExpression,
					thenExpression,
					elseExpression
				);
		}

		protected override ILConstruct EmitAndConditionalExpression( TContext context, IList<ILConstruct> conditionExpressions, ILConstruct thenExpression, ILConstruct elseExpression )
		{
			return
				ILConstruct.IfThenElse(
					ILConstruct.AndCondition( conditionExpressions ),
					thenExpression,
					elseExpression
				);
		}

		protected override ILConstruct EmitStringSwitchStatement( TContext context, ILConstruct target, IDictionary<string, ILConstruct> cases )
		{
			// Simply if statements
			ILConstruct @else = null;
			foreach ( var @case in cases )
			{
				@else =
					this.EmitConditionalExpression(
						context,
						this.EmitInvokeMethodExpression(
							context,
							null,
							Metadata._String.op_Equality,
							target,
							this.MakeStringLiteral( context, @case.Key )
						),
						@case.Value,
						@else
					);
			}

			return @else;
		}

		protected override ILConstruct EmitForLoop( TContext context, ILConstruct count, Func<ForLoopContext, ILConstruct> loopBodyEmitter )
		{
			var i =
				this.DeclareLocal(
					context,
					typeof( int ),
					"i"
				);

			var loopContext = new ForLoopContext( i );
			return
				this.EmitSequentialStatements(
					context,
					i.ContextType,
					i,
					ILConstruct.Instruction(
						"for",
						typeof( void ),
						false,
						il =>
						{
							var forCond = il.DefineLabel( "FOR_COND" );
							il.EmitBr( forCond );
							var body = il.DefineLabel( "BODY" );
							il.MarkLabel( body );
							loopBodyEmitter( loopContext ).Evaluate( il );
							// increment
							i.LoadValue( il, false );
							il.EmitLdc_I4_1();
							il.EmitAdd();
							i.StoreValue( il );
							// cond
							il.MarkLabel( forCond );
							i.LoadValue( il, false );
							count.LoadValue( il, false );
							il.EmitBlt( body );
						}
					)
				);
		}

		protected override ILConstruct EmitForEachLoop( TContext context, CollectionTraits traits, ILConstruct collection, Func<ILConstruct, ILConstruct> loopBodyEmitter )
		{
			return
				ILConstruct.Instruction(
					"foreach",
					typeof( void ),
					false,
					il =>
					{
						var enumerator = il.DeclareLocal( traits.GetEnumeratorMethod.ReturnType, "enumerator" );
						var currentItem =
							this.DeclareLocal( 
								context,
								traits.ElementType,
								"item"
							);

						// gets enumerator
						collection.LoadValue( il, true );

						il.EmitAnyCall( traits.GetEnumeratorMethod );
						il.EmitAnyStloc( enumerator );

						if ( typeof( IDisposable ).IsAssignableFrom( traits.GetEnumeratorMethod.ReturnType ) )
						{
							il.BeginExceptionBlock();
						}

						var startLoop = il.DefineLabel( "START_LOOP" );
						il.MarkLabel( startLoop );
						currentItem.Evaluate( il );

						var endLoop = il.DefineLabel( "END_LOOP" );
						var enumeratorType = traits.GetEnumeratorMethod.ReturnType;
						var moveNextMethod = Metadata._IEnumerator.FindEnumeratorMoveNextMethod( enumeratorType );
						var currentProperty = Metadata._IEnumerator.FindEnumeratorCurrentProperty( enumeratorType, traits );

						Contract.Assert( currentProperty != null, enumeratorType.ToString() );

						// iterates
						if ( traits.GetEnumeratorMethod.ReturnType.IsValueType )
						{
							il.EmitAnyLdloca( enumerator );
						}
						else
						{
							il.EmitAnyLdloc( enumerator );
						}

						il.EmitAnyCall( moveNextMethod );
						il.EmitBrfalse( endLoop );

						// get current item
						if ( traits.GetEnumeratorMethod.ReturnType.IsValueType )
						{
							il.EmitAnyLdloca( enumerator );
						}
						else
						{
							il.EmitAnyLdloc( enumerator );
						}
						il.EmitGetProperty( currentProperty );
						currentItem.StoreValue( il );

						// body
						loopBodyEmitter( currentItem ).Evaluate( il );

						// end loop
						il.EmitBr( startLoop );

						il.MarkLabel( endLoop );

						// Dispose
						if ( typeof( IDisposable ).IsAssignableFrom( traits.GetEnumeratorMethod.ReturnType ) )
						{
							il.BeginFinallyBlock();

							if ( traits.GetEnumeratorMethod.ReturnType.IsValueType )
							{
								var disposeMethod = traits.GetEnumeratorMethod.ReturnType.GetMethod( "Dispose" );
								if ( disposeMethod != null && disposeMethod.GetParameters().Length == 0 && disposeMethod.ReturnType == typeof( void ) )
								{
									il.EmitAnyLdloca( enumerator );
									il.EmitAnyCall( disposeMethod );
								}
								else
								{
									il.EmitAnyLdloc( enumerator );
									il.EmitBox( traits.GetEnumeratorMethod.ReturnType );
									il.EmitAnyCall( Metadata._IDisposable.Dispose );
								}
							}
							else
							{
								il.EmitAnyLdloc( enumerator );
								il.EmitAnyCall( Metadata._IDisposable.Dispose );
							}

							il.EndExceptionBlock();
						}
					}
				);
		}

		protected override Func<SerializationContext, MessagePackSerializer<TObject>> CreateSerializerConstructor( TContext codeGenerationContext )
		{
			return context => codeGenerationContext.Emitter.CreateInstance<TObject>( context );
		}
	}
}