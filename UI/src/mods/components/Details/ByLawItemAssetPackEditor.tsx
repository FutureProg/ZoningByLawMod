import classNames from "classnames";
import styles from "./ByLawItemAssetPackEditor.module.scss";
import { useMapValue, useValue } from "cs2/api";
import { AssetPack, toolbar } from "cs2/bindings";
import { Button } from "cs2/ui";
import { assetPackNameToHash$ } from "mods/bindings";
import { ByLawConstraintType, ByLawItemType } from "mods/types";

export interface ByLawItemAssetPackEditorProps {
    itemType: ByLawItemType;
    itemArr: number[];
    constraintType: ByLawConstraintType;
    onChange?: (newArrayValue: number[]) => void;
}

export default (props: ByLawItemAssetPackEditorProps) => {
    let assetPacks = useValue(toolbar.assetPacks$);
    
    let onButtonToggle = (packHash: number, packName: string) => {
        let newArrayValue = props.itemArr.includes(packHash) ? props.itemArr.filter((v) => v != packHash) : [...props.itemArr, packHash];
        props.onChange && props.onChange(newArrayValue);
    }

    let buttons = assetPacks.map((assetPack) => {
        return <AssetPackButton itemArr={props.itemArr} assetPack={assetPack} onButtonToggle={onButtonToggle} key={assetPack.name}/>
    });

    return (
        <div className={styles.view}>
            {buttons}
        </div>
    );
}

const AssetPackButton = (props: {itemArr: number[], assetPack: AssetPack, onButtonToggle: (hash: number, name: string) => void}) => {
    let isEnabled = props.itemArr.includes(useMapValue(assetPackNameToHash$, props.assetPack.name));
    let assetPackHash = useMapValue(assetPackNameToHash$, props.assetPack.name);
    return (
        <Button variant="icon"
            src={props.assetPack.icon}            
            className={classNames({[styles.buttonOn]: isEnabled}, styles.assetPackButton)} 
            onSelect={() => props.onButtonToggle(assetPackHash, props.assetPack.name)}/>
    );
}