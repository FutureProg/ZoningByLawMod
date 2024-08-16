import { Button, Dropdown, DropdownItem, DropdownToggle, FOCUS_AUTO } from 'cs2/ui';
import styles from './ByLawPropertyView.module.scss';
import { useEffect, useState } from 'react';
import { ByLawConstraintType, ByLawItem, ByLawItemCategory, ByLawItemType, ByLawPropertyOperator, ByLawZoneType } from 'mods/types';
import { ButtonedNumberInput } from '../ButtonedNumberInput';
import ByLawPropertyEditSection from './ByLawPropertyEditSection';
import { getMeasurementString, flagToStringArr, GetDefaultByLawItem, getConstraintTypes, getOperationTypes } from 'mods/utils';
import { Theme } from 'cs2/bindings';
import { getModule } from 'cs2/modding';

const DropdownDefaultStyle: Theme | any = getModule("game-ui/common/input/dropdown/themes/default.module.scss", "classes");

const DropdownStyle: Theme | any = getModule("game-ui/menu/themes/dropdown.module.scss", "classes");

interface _Props {
    byLawItem: ByLawItem;
    onChange: (item: ByLawItem) => void;
    onDelete: () => void;
}

export default ({byLawItem, onChange: onChangeCallback, onDelete: onDeleteCallback} : _Props) => {
    let [editing, setEditing] = useState(false);    
    let [_byLawItem, setByLawItem] = useState(byLawItem);

    // when the parent bylaw item changes, update
    useEffect(() => {
        setByLawItem(byLawItem);
    }, [byLawItem]);

    let splitByUpperCase = (txt: string) => txt.split(/(?=[A-Z])/).join(' ');
    let operationValues = Object.entries(ByLawPropertyOperator)
        .filter(([value, key]) => {
            return !isNaN(Number(value)) && Number(value) > 0;
        })
        .filter(([value, key]) => {
            let x : ByLawPropertyOperator = Number(value);            
            return getOperationTypes(_byLawItem).indexOf(x) >= 0; 
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
        })

    
    let onItemTypeChange = (value: string) => {        
        let num : ByLawItemType = Number(value);         
        let nItem = {
            ...GetDefaultByLawItem(),
            byLawItemType: num            
        };
        nItem.constraintType = getConstraintTypes(nItem.byLawItemType)[0];
        setByLawItem(nItem);        
        onChangeCallback(nItem);
    }

    let dropdownItems = nameValues.map((item, idx) => (
        <DropdownItem key={idx} value={item.value} onChange={onItemTypeChange} closeOnSelect={true}>
            {item.key}
        </DropdownItem>
    ));
    let dropdownContent = (
        <div>{dropdownItems}</div>
    );

    let itemTypeDropdown = (
        <Dropdown theme={DropdownStyle} focusKey={FOCUS_AUTO} content={dropdownContent}>
            <DropdownToggle style={{width: '80%'}}>
                {splitByUpperCase(ByLawItemType[_byLawItem.byLawItemType])}
            </DropdownToggle>            
        </Dropdown>
    );

    let onOperationTypeChange = (value: string) => {
        let opType : ByLawPropertyOperator = Number(value); 
        let nItemVal = {
            ..._byLawItem,
            propertyOperator: opType
        };
        setByLawItem(nItemVal);
        onChangeCallback(nItemVal);
    }

    dropdownItems = operationValues.map((item, idx) => (
        <DropdownItem key={idx} value={item.value} onChange={onOperationTypeChange} closeOnSelect={true}>
            {item.key}
        </DropdownItem>
    ));
    dropdownContent = (
        <div>{dropdownItems}</div>
    );
    let operationsDropdown = (
        <Dropdown theme={DropdownStyle} focusKey={FOCUS_AUTO} content={dropdownContent}>
            <DropdownToggle style={{width: '80%'}}>
                {splitByUpperCase(ByLawPropertyOperator[_byLawItem.propertyOperator])}
            </DropdownToggle>            
        </Dropdown>
    )

    let onPropertyValueChange = (item: ByLawItem) => {                  
        setByLawItem(item);
        onChangeCallback(item);
    }

    let onDelete = () => {
        onDeleteCallback && onDeleteCallback();
    }

    let operatorName = splitByUpperCase(ByLawPropertyOperator[_byLawItem.propertyOperator]);
    let propName = splitByUpperCase(ByLawItemType[_byLawItem.byLawItemType]);

    return (
        <div className={styles.row}>      
            <div className={styles.topBar}>
                <div className={styles.propertyName}>                
                    {!editing? propName : itemTypeDropdown}
                </div>
                <div className={styles.operation}>
                    {!editing? operatorName : operationsDropdown}
                </div>
                <div className={styles.description}><ByLawItemDescription item={_byLawItem}/></div>
                <div className={styles.buttons}>
                    <Button 
                        focusKey={FOCUS_AUTO}
                        onSelect={() => setEditing(!editing)} 
                        variant='icon' 
                        src={'coui://uil/Colored/' + (!editing? 'Pencil.svg' : 'Checkmark.svg')} />
                    <Button focusKey={FOCUS_AUTO} variant='icon' src='coui://uil/Colored/Trash.svg' onSelect={() => onDelete()} /> 
                </div>
            </div>
            <div className={styles.editSection + ' ' + (editing? '' : styles.hidden)}>
                <ByLawPropertyEditSection byLawItem={_byLawItem} isOpen={editing} onChange={onPropertyValueChange} />
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