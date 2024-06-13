import { Button, Dropdown, DropdownItem, DropdownToggle, FOCUS_AUTO } from 'cs2/ui';
import styles from './ByLawPropertyView.module.scss';
import { useState } from 'react';
import { ByLawConstraintType, ByLawItem, ByLawItemCategory, ByLawItemType, ByLawPropertyOperator } from 'mods/types';
import { ButtonedNumberInput } from '../ButtonedNumberInput';
import ByLawPropertyEditSection from './ByLawPropertyEditSection';

export default ({byLawItem} : {byLawItem: ByLawItem}) => {
    let [editing, setEditing] = useState(false);    

    let operationValues = Object.entries(ByLawPropertyOperator)
        .filter(([value, key]) => {
            return !isNaN(Number(value)) && Number(value) > 0;
        })
        .map(([value, key], index) => {        
            return {key: (key as string).split(/(?=[A-Z])/).join(' '), value}
        });

    let nameValues = Object.entries(ByLawItemType)
        .filter(([value, key]) => {
            return !isNaN(Number(value)) && Number(value) > 0;
        })
        .map(([value, key], index) => {        
            return {key: (key as string).split(/(?=[A-Z])/).join(' '), value}
        });


    let dropdownItems = nameValues.map((item, idx) => (
        <DropdownItem key={idx} value={item.value}>{item.key}</DropdownItem>
    ));
    let dropdownContent = (
        <div>{dropdownItems}</div>
    );

    let nameDropdown = (
        <Dropdown focusKey={FOCUS_AUTO} content={dropdownContent}>
            <DropdownToggle style={{width: '80%'}}>
                Height
            </DropdownToggle>            
        </Dropdown>
    );

    dropdownItems = operationValues.map((item, idx) => (
        <DropdownItem key={idx} value={item.value}>{item.key}</DropdownItem>
    ));
    dropdownContent = (
        <div>{dropdownItems}</div>
    );
    let operationsDropdown = (
        <Dropdown focusKey={FOCUS_AUTO} content={dropdownContent}>
            <DropdownToggle style={{width: '80%'}}>
                Is
            </DropdownToggle>            
        </Dropdown>
    )

    

    return (
        <div className={styles.row}>      
            <div className={styles.topBar}>
                <div className={styles.propertyName}>                
                    {!editing? "Height" : nameDropdown}
                </div>
                <div className={styles.operation}>
                    {!editing? "is" : operationsDropdown}
                </div>
                <div className={styles.description}>betweeen 10 metres and 20 metres</div>
                <div className={styles.buttons}>
                    <Button onSelect={() => setEditing(!editing)} variant='icon' src='coui://uil/Colored/Pencil.svg' />
                    <Button variant='icon' src='coui://uil/Colored/Trash.svg' />                
                </div>
            </div>
            <div className={styles.editSection + ' ' + (editing? '' : styles.hidden)}>
                <ByLawPropertyEditSection byLawItem={byLawItem} isOpen={editing} />
            </div>
        </div>
    )
}