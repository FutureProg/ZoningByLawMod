import { bindValue, trigger } from "cs2/api";
import mod from '../../mod.json';
import { ByLawZoneComponent, ByLawZoneData, ByLawZoneListItem } from "./types";
import { Entity } from "cs2/bindings";


export const byLawZoneList$ = bindValue<ByLawZoneListItem[]>(mod.fullname, "ByLawZoneList", []);
export const selectedByLawData$ = bindValue<ByLawZoneComponent>(mod.fullname, "SelectedByLawData");

export const setActiveByLaw = (entity: Entity) => {
    trigger(mod.fullname, "SetActiveByLaw", entity);
}

export const setByLawData = (byLawData: ByLawZoneComponent) => {
    trigger(mod.fullname, "SetByLawData", byLawData);
}

export const createNewByLaw = (byLawData : ByLawZoneComponent) => {
    trigger(mod.fullname, "CreateNewByLaw", byLawData);
}

export const deleteByLaw = (entity: Entity) => {
    trigger(mod.fullname, "DeleteByLaw", entity);
}