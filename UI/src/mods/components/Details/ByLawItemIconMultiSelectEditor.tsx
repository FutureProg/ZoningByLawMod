import styles from "./ByLawItemIconMultiSelectEditor.module.scss";
import { Tooltip } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { FieldDataOption } from "mods/bindings";
import { ByLawConstraintType, ByLawItemType } from "mods/types";
import { VanillaComponentResolver } from "vanillacomponentresolver";

export interface ByLawItemIconMultiSelectEditorProps {
    itemType: ByLawItemType;
    itemArr: number[];
    constraintType: ByLawConstraintType;
    options: FieldDataOption[];
    onChange?: (newArrayValue: number[]) => void;
}

export default (props: ByLawItemIconMultiSelectEditorProps) => {
    let onButtonToggle = (hash: number) => {
        let newArrayValue = props.itemArr.includes(hash) ? props.itemArr.filter((v) => v != hash) : [...props.itemArr, hash];
        props.onChange && props.onChange(newArrayValue);
    }

    let buttons = props.options.map((option) => {
        return <IconMultiSelectButton itemArr={props.itemArr} option={option} onButtonToggle={onButtonToggle} key={option.value}/>
    });

    return (
        <div className={styles.view}>
            {buttons}
        </div>
    );
}

const IconMultiSelectButton = (props: {itemArr: number[], option: FieldDataOption, onButtonToggle: (hash: number) => void}) => {
    let { translate } = useLocalization();
    let isEnabled = props.itemArr.includes(props.option.value);
    let ToolButton = VanillaComponentResolver.instance.ToolButton;
    return (
        <Tooltip tooltip={translate(props.option.label, props.option.label)} direction="up">
            <ToolButton
                src={props.option.image ?? ""}
                selected={isEnabled}
                onSelect={() => props.onButtonToggle(props.option.value)}/>
        </Tooltip>
    );
}
