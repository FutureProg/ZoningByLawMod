import { bindValue, trigger } from "cs2/api";
import mod from '../../mod.json';
import { ByLawZoneComponent, ByLawZoneData, ByLawZoneListItem } from "./types";


export const byLawZoneList = bindValue<ByLawZoneListItem[]>(mod.fullname, "ByLawZoneList", []);
export const selectedByLawData = bindValue<ByLawZoneComponent>(mod.fullname, "ByLaws");

export const setByLawData = (byLawData: ByLawZoneComponent) => {
    trigger(mod.fullname, "SetByLawData", byLawData);
}