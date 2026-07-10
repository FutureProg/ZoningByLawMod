import styles from "./ByLawItemAssetPackEditor.module.scss";
import { Button } from "cs2/ui";
import { FieldDataOption } from "mods/bindings";
import { ByLawConstraintType, ByLawItemType } from "mods/types";

export interface ByLawItemAssetPackEditorProps {
    itemType: ByLawItemType;
    itemArr: number[];
    constraintType: ByLawConstraintType;
    options: FieldDataOption[];
    onChange?: (newArrayValue: number[]) => void;
}

export default (props: ByLawItemAssetPackEditorProps) => {
    let onButtonToggle = (packHash: number) => {
        let newArrayValue = props.itemArr.includes(packHash) ? props.itemArr.filter((v) => v != packHash) : [...props.itemArr, packHash];
        props.onChange && props.onChange(newArrayValue);
    }

    let buttons = props.options.map((option) => {
        return <AssetPackButton itemArr={props.itemArr} option={option} onButtonToggle={onButtonToggle} key={option.value}/>
    });

    return (
        <div className={styles.view}>
            {buttons}
        </div>
    );
}

const AssetPackButton = (props: {itemArr: number[], option: FieldDataOption, onButtonToggle: (hash: number) => void}) => {
    let isEnabled = props.itemArr.includes(props.option.value);
    return (
        <Button variant="icon"
            src={props.option.image}
            selected={isEnabled}
            className={styles.assetPackButton}
            onSelect={() => props.onButtonToggle(props.option.value)}/>
    );
}
