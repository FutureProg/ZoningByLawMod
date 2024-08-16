import { ByLawItem, ByLawItemType } from 'mods/types';
import styles from './ConstraintListItem.module.scss';
import checkboxTheme from '../../themes/RoundCheckboxTheme.module.scss';
import { VanillaComponentResolver } from 'vanillacomponentresolver';
import { useState } from 'react';
import ConstraintValueText from '../ConstraintValueText/ConstraintValueText';
import ByLawPropertyEditSection from '../Details/ByLawPropertyEditSection';

type ConstraintListItemProps = {
    itemType: ByLawItemType,
    value?: ByLawItem,
    readableName: string,
    onChecked?: (newValue: boolean) => void
}

export const ConstraintListItem = (props: ConstraintListItemProps) => {
    let [isOpen, setIsOpen] = useState(false);
    let toggleOpen = () => setIsOpen(!isOpen);
    let enabled = props.value != undefined;    
    return (
        <div className={styles.view} onClick={toggleOpen}>
            <div className={styles.infoRow}>
                <VanillaComponentResolver.instance.Checkbox
                    theme={checkboxTheme}
                    onChange={() => { props.onChecked && props.onChecked(!enabled) }}
                    checked={enabled}
                />
                <div className={styles.constraintName}>{props.readableName}</div>
                <div className={styles.operator}>{enabled ? "is" : ""}</div>
                <ConstraintValueText className={styles.valueDescription} item={props.value} />
            </div>
            <div className={styles.editorSection}>
                {enabled ?
                    <ByLawPropertyEditSection byLawItem={props.value!} isOpen={isOpen} />
                    : <></>
                }
            </div>
            <div className={styles.caretSection}>
                <div></div>
            </div>
        </div>
    )
}