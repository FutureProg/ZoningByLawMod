import { ByLawConstraintType, ByLawItemType, ByLawZoneType, PollutionValues } from 'mods/types';
import EnumFieldCheckboxes from '../EnumFieldCheckboxes';
import styles from './ByLawItemEnumEditor.module.scss';
import { useMemo, useState } from 'react';
import { CheckboxFieldData, RadioFieldData } from 'mods/bindings';

export interface ByLawItemEnumEditorProps {
    itemType: ByLawItemType;
    fieldData: CheckboxFieldData | RadioFieldData
    constraintType: ByLawConstraintType;
    onChange?: (enumValue: any) => void;
};

export default (props: ByLawItemEnumEditorProps) => {        
    let selectType : 'multi' | 'single' = props.fieldData.fieldType === "checkbox"? 'multi' : 'single';
    //props.constraintType == ByLawConstraintType.MultiSelect? 'multi' : 'single';

    let [editorValue, setEditorValue] = useState(props.fieldData.value as number[] | number);
    let onChange = (nArray: any) => {
        setEditorValue(nArray);
        props.onChange?.call(null, nArray);
    }
    let childProps = {  
        options: props.fieldData.options!,
        value: Array.isArray(editorValue) ? editorValue : [editorValue],      
        type: selectType,
        onChange: onChange
    };
    let field = useMemo(() => {
        return EnumFieldCheckboxes({
            ...childProps
        });
    }, [props.itemType, props.constraintType, onChange, editorValue, props.fieldData.id]);

    return (
        <div className={styles.view}>
            {field}
        </div>
    )
};