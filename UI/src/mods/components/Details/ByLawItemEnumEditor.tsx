import { ByLawConstraintType, ByLawItem, ByLawItemType, ByLawZoneType } from 'mods/types';
import EnumFieldCheckboxes, { EnumFieldCheckboxesProps } from '../EnumFieldCheckboxes';
import styles from './ByLawItemEnumEditor.module.scss';
import { useMemo, useState } from 'react';

export interface ByLawItemEnumEditorProps {
    itemType: ByLawItemType;
    itemValue: number;
    constraintType: ByLawConstraintType;
    onChange?: (enumValue: any) => void;
};

enum bad {}

export default (props: ByLawItemEnumEditorProps) => {        
    let selectType : 'multi' | 'single' = props.constraintType == ByLawConstraintType.MultiSelect? 'multi' : 'single';


    let [editorValue, setEditorValue] = useState(props.itemValue);
    let onChange = (nEnumValue: any) => {
        setEditorValue(nEnumValue);
        props.onChange?.call(null, nEnumValue);
    }
    let childProps= {        
        type: selectType,
        onChange: onChange
    };
    let field = useMemo(()=>{
        switch(props.itemType) {
            case ByLawItemType.LandUse:
                return EnumFieldCheckboxes<ByLawZoneType>({
                    enum: editorValue as ByLawZoneType,
                    enumEntries: Object.entries(ByLawZoneType),
                    ...childProps
                });
            default:
                return (<></>);
        }
    }, [props.itemType, props.constraintType, onChange, editorValue]);

    return (
        <div className={styles.view}>
            {field}
        </div>
    )
};