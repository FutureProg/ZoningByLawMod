import { Scrollable } from 'cs2/ui';
import styles from './ByLawEditorView.module.scss';
import { useValue } from 'cs2/api';
import { selectedByLawColor$, selectedByLawData$, selectedByLawName$, setByLawName } from 'mods/bindings';
import { ByLawItemType } from 'mods/types';
import { ConstraintListItem } from 'mods/components/ConstraintListItem/ConstraintListItem';
import { ChangeEvent, useEffect, useMemo, useState } from 'react';
import { TextInputTheme } from 'mods/components/TextInput/TextInput';
import classNames from 'classnames';
import ImageLabelButton from 'mods/atoms/ImageLabelButton';
import { VanillaComponentResolver } from 'vanillacomponentresolver';
import { Color } from 'cs2/bindings';
import { rgbaToHex } from 'mods/utils';

export const ByLawEditorView = ({ searchQuery }: { searchQuery?: string }) => {
    let byLawData = useValue(selectedByLawData$);
    let byLawName = useValue(selectedByLawName$);
    let byLawColor = useValue(selectedByLawColor$);
    let [_byLawName, set_ByLawName] = useState(byLawName);

    useEffect(() => {
        set_ByLawName(byLawName);
    }, [byLawName]);
    

    let onNameChange = ({target} : ChangeEvent<HTMLInputElement>) => {
        setByLawName(target.value);
    }

    let onColorChange = (col: Color) => {

    }
    let onColorChangeHex = ({target} : ChangeEvent<HTMLInputElement>) => {
        // Do hex stuff, convert to Color, then call onColorChange
    }

    let items = byLawData.blocks[0].itemData;
    let itemMap = useMemo(() =>
        Object.fromEntries(
            items.map((item) => [ByLawItemType[item.byLawItemType], item])
        )
    , [items]);
    let types = Object.keys(ByLawItemType)        
        .filter((key) => isNaN(Number(key)) && key != 'None')
        .map((key) => [key, key.split(/(?<![A-Z])(?=[A-Z])/).join(' ')] as [keyof typeof ByLawItemType, string])        
        .filter(([key, readableName]) => searchQuery && readableName ? readableName.toUpperCase().indexOf(searchQuery.toUpperCase()) > 0 : true);
    let listItems = types.map(([key, readableName]: [keyof typeof ByLawItemType, string], idx) =>
        <ConstraintListItem
            key={idx}
            readableName={readableName}
            itemType={ByLawItemType[key]}
            value={itemMap[key] || undefined}
        />
    );    

    return (
        <Scrollable className={styles.view}>
            <div className={styles.nameItem}>
                <label>Name</label>
                <input 
                    type={'text'} 
                    value={_byLawName} 
                    className={classNames(TextInputTheme.input, styles.textBox)}
                    onChange={({target}) => set_ByLawName(target.value)}         
                    onBlur={onNameChange}
                />
            </div>
            <div className={styles.colorItem}>
                <label>Colour</label>
                <div>
                    <VanillaComponentResolver.instance.ColorField 
                        value={byLawColor[0]} 
                        onChange={onColorChange}
                        className={styles.colorButton}
                    />
                    <input 
                        type={'text'} 
                        value={rgbaToHex(byLawColor[0])} 
                        className={classNames(TextInputTheme.input, styles.textBox)}
                        onChange={onColorChangeHex}
                    />                                    
                </div>
            </div>
            {listItems}
        </Scrollable>
    )
}