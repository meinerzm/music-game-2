/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2020
 *	
 *	"ActionContainerOpen.cs"
 * 
 *	This action makes a Container active for display in an
 *	InventoryBox. To de-activate it, close a Menu with AppearType
 *	set to OnContainer.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionContainerOpen : Action
	{

		public bool useActive = false;
		public int parameterID = -1;
		public int constantID = 0;
		public Container container;
		protected Container runtimeContainer;

		public bool setElement;
		public string menuName;
		public string containerElementName;

		public int menuParameterID = -1;
		public int elementParameterID = -1;

		protected LocalVariables localVariables;
		protected MenuInventoryBox runtimeInventoryBox;
		

		public ActionContainerOpen ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Container;
			title = "Open";
			description = "Opens a chosen Container, causing any Menu of Appear type: On Container to open. To close the Container, simply close the Menu.";
		}


		public override void AssignParentList (ActionList actionList)
		{
			if (actionList != null)
			{
				localVariables = UnityVersionHandler.GetLocalVariablesOfGameObject (actionList.gameObject);
			}
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}

			base.AssignParentList (actionList);
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			if (useActive)
			{
				runtimeContainer = KickStarter.playerInput.activeContainer;
			}
			else
			{
				runtimeContainer = AssignFile <Container> (parameters, parameterID, constantID, container);
			}

			if (!useActive && setElement && !string.IsNullOrEmpty (menuName) && !string.IsNullOrEmpty (containerElementName))
			{
				string runtimeMenuName = AssignString (parameters, menuParameterID, menuName);
				string runtimeContainerElementName = AssignString (parameters, elementParameterID, containerElementName);

				runtimeMenuName = AdvGame.ConvertTokens (runtimeMenuName, Options.GetLanguage (), localVariables, parameters);
				runtimeContainerElementName = AdvGame.ConvertTokens (runtimeContainerElementName, Options.GetLanguage (), localVariables, parameters);
				
				MenuElement element = PlayerMenus.GetElementWithName (runtimeMenuName, runtimeContainerElementName);
				if (element != null)
				{
					runtimeInventoryBox = element as MenuInventoryBox;
				}
			}
		}

		
		public override float Run ()
		{
			if (runtimeContainer && runtimeContainer.enabled && runtimeContainer.gameObject.activeInHierarchy)
			{
				if (!useActive && setElement)
				{
					if (runtimeInventoryBox != null)
					{
						runtimeInventoryBox.OverrideContainer = runtimeContainer;
						return 0f;
					}

					LogWarning ("Could not find an InventoryBox element '" + containerElementName + "' in Menu '" + menuName + "'");
				}
				else
				{
					runtimeContainer.Interact ();
				}
			}

			return 0f;
		}
		

		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			numSockets = 0;
			useActive = EditorGUILayout.Toggle ("Affect active container?", useActive);
			if (!useActive)
			{
				parameterID = Action.ChooseParameterGUI ("Container:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					container = null;
				}
				else
				{
					container = (Container) EditorGUILayout.ObjectField ("Container:", container, typeof (Container), true);
					
					constantID = FieldToID <Container> (container, constantID);
					container = IDToField <Container> (container, constantID, false);
				}

				setElement = EditorGUILayout.Toggle ("Open in set element?", setElement);
				if (setElement)
				{
					menuParameterID = Action.ChooseParameterGUI ("Menu name:", parameters, menuParameterID, ParameterType.String);
					if (menuParameterID < 0)
					{
						menuName = EditorGUILayout.TextField ("Menu name:", menuName);
					}

					elementParameterID = Action.ChooseParameterGUI ("InventoryBox name:", parameters, elementParameterID, ParameterType.String);
					if (elementParameterID < 0)
					{
						containerElementName = EditorGUILayout.TextField ("InventoryBox name:", containerElementName);
					}
					numSockets = 1;
				}
			}

			if (numSockets == 1)
			{
				AfterRunningOption ();
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <RememberContainer> (container);
			}
			AssignConstantID <Container> (container, constantID, parameterID);
		}

		
		public override string SetLabel ()
		{
			if (container != null)
			{
				return container.name;
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject _gameObject, int id)
		{
			if (!useActive && parameterID < 0)
			{
				if (container != null && container.gameObject == _gameObject) return true;
				if (constantID == id) return true;
			}
			return false;
		}

		#endif


		/**
		* <summary>Creates a new instance of the 'Containter: Open' Action</summary>
		* <param name = "containerToOpen">The Container to open</param>
		* <returns>The generated Action</returns>
		*/
		public static ActionContainerOpen CreateNew (Container containerToOpen)
		{
			ActionContainerOpen newAction = (ActionContainerOpen) CreateInstance <ActionContainerOpen>();
			newAction.container = containerToOpen;
			return newAction;
		}
		
	}

}