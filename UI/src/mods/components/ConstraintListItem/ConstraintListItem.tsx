import { ByLawItem, ByLawItemType } from 'mods/types';
import styles from './ConstraintListItem.module.scss';
import { VanillaComponentResolver } from 'vanillacomponentresolver';
import { useState } from 'react';

export const ConstraintListItem = (props: {itemType: ByLawItemType, value?: ByLawItem}) => {
    
    let [checked, setChecked] = useState(false);

    let checkboxTheme = {
        toggle: styles.checkboxToggle,
        checkmark: styles.checkboxMark
    }

    return (
        <div className={styles.view}>
            <VanillaComponentResolver.instance.Checkbox theme={checkboxTheme} onChange={() => setChecked(!checked)} checked={checked} />
            {String(props.itemType)}
        </div>
    )
}