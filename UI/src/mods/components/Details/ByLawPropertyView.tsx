import { Button, Dropdown, DropdownItem, DropdownToggle, FOCUS_AUTO } from 'cs2/ui';
import styles from './ByLawPropertyView.module.scss';
import { useState } from 'react';
import { ByLawConstraintType, ByLawItem, ByLawItemCategory, ByLawItemType, ByLawPropertyOperator, ByLawZoneType } from 'mods/types';
import { ButtonedNumberInput } from '../ButtonedNumberInput';
import ByLawPropertyEditSection from './ByLawPropertyEditSection';
import { getMeasurementString, flagToStringArr } from 'mods/utils';

export default ({byLawItem} : {byLawItem: ByLawItem}) => {
    let [editing, setEditing] = useState(false);    
    let [_byLawItem, setByLawItem] = useState(byLawItem);

    let splitByUpperCase = (txt: string) => txt.split(/(?=[A-Z])/).join(' ');
    let operationValues = Object.entries(ByLawPropertyOperator)
        .filter(([value, key]) => {
            return !isNaN(Number(value)) && Number(value) > 0;
        })
        .map(([value, key], index) => {        
            return {key: splitByUpperCase(key as string), value}
        });

    let nameValues = Object.entries(ByLawItemType)
        .filter(([value, key]) => {
            return !isNaN(Number(value)) && Number(value) > 0;
        })
        .map(([value, key], index) => {        
            return {key: splitByUpperCase(key as string), value}
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

    let operatorName = splitByUpperCase(ByLawPropertyOperator[_byLawItem.propertyOperator]);
    let propName = splitByUpperCase(ByLawItemType[_byLawItem.byLawItemType]);

    return (
        <div className={styles.row}>      
            <div className={styles.topBar}>
                <div className={styles.propertyName}>                
                    {!editing? propName : nameDropdown}
                </div>
                <div className={styles.operation}>
                    {!editing? operatorName : operationsDropdown}
                </div>
                <div className={styles.description}><ByLawItemDescription item={_byLawItem}/></div>
                <div className={styles.buttons}>
                    <Button onSelect={() => setEditing(!editing)} variant='icon' src='coui://uil/Colored/Pencil.svg' />
                    <Button variant='icon' src='coui://uil/Colored/Trash.svg' />                
                </div>
            </div>
            <div className={styles.editSection + ' ' + (editing? '' : styles.hidden)}>
                <ByLawPropertyEditSection byLawItem={_byLawItem} isOpen={editing} />
            </div>
        </div>
    )
}

const ByLawItemDescription = ({item} : {item: ByLawItem}) => {
    let txt = "";
    let measure = getMeasurementString(item.byLawItemType, item.constraintType);
    switch(item.constraintType) {        
        case ByLawConstraintType.Length:            
            txt = `Between ${item.valueBounds1.min}${measure} and ${item.valueBounds1.max}${measure}`;
            break;                      
        case ByLawConstraintType.MultiSelect:
        case ByLawConstraintType.SingleSelect:
            txt = flagToStringArr(item.valueByteFlag, item.byLawItemType).join(', ');
            break;
        case ByLawConstraintType.Count:   
            txt = `${item.valueNumber}${measure}`;
        case ByLawConstraintType.None:
        default:
            return (<p></p>)
    }
    return (<p>{txt}</p>);
}