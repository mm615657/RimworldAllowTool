﻿using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AllowTool {
	
	/// <summary>
	/// Base class for custom designators that deal with selectable Things.
	/// This mainly exists to allow the use of an alternative DesignationDragger.
	/// </summary>
	public abstract class Designator_SelectableThings : Designator {
		internal readonly ThingDesignatorDef def;
		protected int numThingsDesignated;

		public override int DraggableDimensions {
			get { return 2; }
		}

		public override bool DragDrawMeasurements {
			get { return true; }
		}

		private bool visible = true;
		public override bool Visible {
			get { return visible; }
		}

		protected Designator_SelectableThings(ThingDesignatorDef def) {
			this.def = def;
			defaultLabel = def.label;
			defaultDesc = def.description;
			icon = def.IconTex;
			useMouseIcon = true;
			soundDragSustain = SoundDefOf.DesignateDragStandard;
			soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			soundSucceeded = def.soundSucceeded;
			hotKey = def.hotkeyDef;
		}

		public void SetVisible(bool value) {
			visible = value;
		}

		// this is called by the vanilla DesignationDragger. We are using UnlimitedDesignationDragger instead.
		// returning false prevents the vanilla dragger from selecting any of the cells.
		public override AcceptanceReport CanDesignateCell(IntVec3 c) {
			return false;
		}
		
		public override void Selected() {
			AllowToolController.Instance.Dragger.BeginListening(CanDesignateThing, def.DragHighlightTex);
		}

		public override void DesignateSingleCell(IntVec3 cell) {
			numThingsDesignated = 0;
			var map = Find.VisibleMap;
			if (map == null) return;
			var things = map.thingGrid.ThingsListAt(cell);
			for (int i = 0; i < things.Count; i++) {
				var t = things[i];
				if (CanDesignateThing(t).Accepted) {
					DesignateThing(t);
					numThingsDesignated++;
				}
			}
		}

		public override void DesignateMultiCell(IEnumerable<IntVec3> cells) {
			var hitCount = 0;
			foreach (var cell in AllowToolController.Instance.Dragger.GetAffectedCells()) {
				DesignateSingleCell(cell);
				hitCount += numThingsDesignated;
			}
			if (hitCount > 0) {
				if (def.messageSuccess != null) Messages.Message(def.messageSuccess.Translate(hitCount.ToString()), MessageTypeDefOf.SilentInput);
				FinalizeDesignationSucceeded();
			} else {
				if (def.messageFailure != null) Messages.Message(def.messageFailure.Translate(), MessageTypeDefOf.RejectInput);
				FinalizeDesignationFailed();
			}
		}

		public virtual void SelectedOnGUI() {
		}
	}
}
