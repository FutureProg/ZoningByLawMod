import { ByLawItem, ByLawItemType } from 'mods/types';
import styles from './ConstraintListItem.module.scss';
import checkboxTheme from '../../themes/RoundCheckboxTheme.module.scss';
import { VanillaComponentResolver } from 'vanillacomponentresolver';
import { useState } from 'react';
import ConstraintValueText from '../ConstraintValueText/ConstraintValueText';
import ByLawPropertyEditSection from '../Details/ByLawPropertyEditSection';
import classNames from 'classnames';

type ConstraintListItemProps = {
    itemType: ByLawItemType,
    value?: ByLawItem,
    readableName: string,
    onChangeConstraintEnabled?: (newValue: boolean, itemType: ByLawItemType) => void
    onValueChange?: (newItemValue: ByLawItem) => void;
}

export const ConstraintListItem = (props: ConstraintListItemProps) => {
    let [isOpen, setIsOpen] = useState(false);
    let toggleOpen = () => setIsOpen(!isOpen);
    let enabled = props.value != undefined;   
    
    let onItemChange = (newItemValue: ByLawItem) => {
        props.onValueChange && props.onValueChange(newItemValue);
    }
    return (
        <div className={styles.view} onClick={toggleOpen}>
            <div className={styles.infoRow}>
                <VanillaComponentResolver.instance.Checkbox
                    theme={checkboxTheme}
                    onChange={() => { props.onChangeConstraintEnabled && props.onChangeConstraintEnabled(!enabled, props.itemType) }}
                    checked={enabled}
                />
                <div className={styles.constraintName}>{props.readableName}</div>
                <div className={styles.operator}>{enabled ? "is" : ""}</div>
                <ConstraintValueText className={styles.valueDescription} item={props.value} />
            </div>
            <div className={classNames(styles.editorSection, {[styles.open]: isOpen})}>
                {enabled ?
                    <ByLawPropertyEditSection byLawItem={props.value!} isOpen={isOpen} onChange={onItemChange} />
                    : <></>
                }
            </div>
            <div className={styles.caretSection}>
                <div></div>
            </div>
        </div>
    )
}