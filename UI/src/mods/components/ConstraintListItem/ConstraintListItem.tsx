import { ByLawItem, ByLawItemType, ByLawPropertyOperator } from 'mods/types';
import styles from './ConstraintListItem.module.scss';
import checkboxTheme from '../../themes/RoundCheckboxTheme.module.scss';
import { VanillaComponentResolver } from 'vanillacomponentresolver';
import { useState } from 'react';
import ConstraintValueText from '../ConstraintValueText/ConstraintValueText';
import ByLawPropertyEditSection from '../Details/ByLawPropertyEditSection';
import classNames from 'classnames';
import ConstraintOperatorText from '../ConstraintOperatorText/ConstraintOperatorText';
import { useLocalization } from 'cs2/l10n';
import { Dropdown, DropdownItem, DropdownToggle, FOCUS_DISABLED } from 'cs2/ui';
import { deepCopy, getOperationTypes } from 'mods/utils';
import { Theme } from 'cs2/bindings';
import { getModule } from 'cs2/modding';
import { useValue } from 'cs2/api';
import { byLawFields$, FieldDataBase } from 'mods/bindings';

const DropdownStyle: Theme | any = getModule("game-ui/menu/themes/dropdown.module.scss", "classes");

type ConstraintListItemProps = {
    itemType: ByLawItemType,
    value?: ByLawItem,
    readableName: string,
    fieldData: FieldDataBase,
    onChangeConstraintEnabled?: (newValue: boolean, itemType: ByLawItemType) => void
    onValueChange?: (newItemValue: ByLawItem) => void;
}

export const ConstraintListItem = (props: ConstraintListItemProps) => {
    let [isOpen, setIsOpen] = useState(false);    
    let enabled = props.value != undefined;   
    let {translate} = useLocalization();    

    let toggleOpen = () => {
        if (!enabled && props.onChangeConstraintEnabled) {
            props.onChangeConstraintEnabled(!enabled, props.itemType);
            setIsOpen(true);
        } 
        else if (enabled) {
            setIsOpen(!isOpen);
        } else {
            setIsOpen(false);
        }     
    }
    
    let onChangeEnabled = () => {
        setIsOpen(false);
        props.onChangeConstraintEnabled && props.onChangeConstraintEnabled(!enabled, props.itemType)
    }

    let onItemChange = (newItemValue: ByLawItem) => {
        props.onValueChange && props.onValueChange(newItemValue);
    }

    let onPropertyOperatorChange = (operator: ByLawPropertyOperator) => {
        onItemChange({
            ...deepCopy(props.value!),
            propertyOperator: operator
        });
    }

    const operatorOptions = getOperationTypes(props.itemType).map((operator, index) => {
        return (
            <DropdownItem 
                focusKey={FOCUS_DISABLED}
                onChange={onPropertyOperatorChange} 
                value={operator} 
                key={props.itemType + " " + operator}
                selected={operator == props.value?.propertyOperator}>
                {translate(`ZBL.PropertyOperator[${ByLawPropertyOperator[operator]}]`)}
            </DropdownItem>
        )
    });
    const currentOperatorText = props.value? translate(`ZBL.PropertyOperator[${ByLawPropertyOperator[props.value.propertyOperator]}]`) : '';    
    return (
        <div className={styles.view} onClick={toggleOpen}>
            <div className={styles.infoRow}>
                <VanillaComponentResolver.instance.Checkbox
                    theme={checkboxTheme}
                    onChange={onChangeEnabled}
                    checked={enabled}
                />
                <div className={styles.constraintName}>{translate(props.fieldData.label, props.fieldData.label)}</div>
                <ConstraintOperatorText className={styles.operator} item={props.value}/>
                <ConstraintValueText className={styles.valueDescription} item={props.value} />
            </div>
            <div className={classNames(styles.editorSection, {[styles.open]: isOpen && enabled})}>
                {enabled ?
                    <>
                    { operatorOptions.length > 1 && isOpen? 
                    <Dropdown focusKey={FOCUS_DISABLED} theme={DropdownStyle} content={operatorOptions}>
                        <DropdownToggle>
                            {currentOperatorText}
                        </DropdownToggle>                        
                    </Dropdown>: null
                    }
                    <ByLawPropertyEditSection byLawItem={props.value!} fieldData={props.fieldData} isOpen={isOpen} onChange={onItemChange} />
                    </>                    
                    : <></>
                }
            </div>
            <div className={styles.caretSection}>
                <div style={{"maskImage": "url('Media/Glyphs/StrokeArrowDown.svg');"}}></div>
            </div>
        </div>
    )
}