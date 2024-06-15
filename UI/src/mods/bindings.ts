import { bindValue, trigger } from "cs2/api";
import mod from '../../mod.json';
import { ByLawZoneListItem, ZoningByLawBinding } from "./types";
import { Color, Entity } from "cs2/bindings";

export const ZONE_COLOR_IDX = 0;
export const ZONE_BORDER_IDX = 1;

export const byLawZoneList$ = bindValue<ByLawZoneListItem[]>(mod.fullname, "ByLawZoneList", []);
export const selectedByLawData$ = bindValue<ZoningByLawBinding>(mod.fullname, "SelectedByLawData");
export const isConfigPanelOpen$ = bindValue<boolean>(mod.fullname, "IsConfigPanelOpen");
export const selectedByLawName$ = bindValue<string>(mod.fullname, "SelectedByLawName", "");
export const defaultColor = {r: 1, g: 1, b: 1, a: 1};
export const selectedByLawColor$ = bindValue<Color[]>(mod.fullname, "SelectedByLawColour", [defaultColor, defaultColor]);
export const selectedByLaw$ = bindValue<Entity>(mod.fullname, "SelectedByLaw");

export const setConfigPanelOpen = (open : boolean) => {
    trigger(mod.fullname, "SetConfigPanelOpen", open);
}

export const setActiveByLaw = (entity: Entity) => {
    trigger(mod.fullname, "SetActiveByLaw", entity);
}

export const setByLawData = (byLawData: ZoningByLawBinding) => {
    trigger(mod.fullname, "SetByLawData", byLawData);
}

export const createNewByLaw = (byLawData : ZoningByLawBinding) => {
    console.log(byLawData);
    trigger(mod.fullname, "CreateNewByLaw", byLawData);
}

export const deleteByLaw = () => {
    trigger(mod.fullname, "DeleteByLaw");
}

export const setByLawName = (name: string) => {
    trigger(mod.fullname, "SetByLawName", name);
}

export const setByLawZoneColor = (zoneColor: Color, borderColor: Color) => {
    trigger(mod.fullname, "SetByLawZoneColour", zoneColor, borderColor);
}

export const toggleByLawRenderPreview = () => {
    trigger(mod.fullname, "ToggleByLawRenderPreview");
}