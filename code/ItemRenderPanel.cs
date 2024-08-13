using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public class ItemRenderPanel : Panel
	{
		readonly ScenePanel scenePanel;

		public string ItemId { get; set; }

		private string PrevItemId;

		private SceneModel SceneModel;
		readonly SceneWorld SceneWorld;

		public ItemRenderPanel()
		{
			Style.FlexWrap = Wrap.Wrap;
			Style.JustifyContent = Justify.Center;
			Style.AlignItems = Align.Center;
			Style.AlignContent = Align.Center;
			Style.Padding = 0;
			//Style.SetBackgroundImage( "/content/background.png" );
			Style.Width = Length.Percent( 100 );
			Style.Height = Length.Percent( 100 );

			SceneWorld  = new SceneWorld();
			scenePanel = new ScenePanel();
			scenePanel.World = SceneWorld;
			
			scenePanel.Camera.FieldOfView = 70;
			
			scenePanel.Style.Width = Length.Percent( 100 );
			scenePanel.Style.Height = Length.Percent( 100 );			

			AddChild( scenePanel );

			
			
		}
		public override void Tick()
		{
			base.Tick();
			if(SceneModel == null || ItemId != PrevItemId )
			{
				if(SceneModel != null )
				{
					SceneModel.Delete();
				}
				Item item = Game.ActiveScene.GetAllComponents<ItemRegistry>().First().Items[ItemId];
				GameObject clone = item.Prefab.Clone();
				Transform cameraTransform = clone.Children.Find( go => go.Name == "invview" ).Transform.World;
				ModelRenderer modelRenderer = clone.Components.GetAll<ModelRenderer>().First();
				Model model = modelRenderer.Model;

				SceneModel = new SceneModel( SceneWorld, model, modelRenderer.Transform.World );
				SceneModel.SetMaterialOverride( modelRenderer.MaterialOverride );
				clone.Destroy();
				new SceneLight( SceneWorld, cameraTransform.Position, 20000, Color.White * 3 );

				scenePanel.Camera.Rotation = /*Rotation.LookAt( -cameraPosition )*/cameraTransform.Rotation;
				scenePanel.Camera.Position = cameraTransform.Position;

				PrevItemId = ItemId;
			}
		}

	}
}
