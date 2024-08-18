import { Scrollable } from 'cs2/ui';
import styles from './ByLawEditorView.module.scss';
import { useValue } from 'cs2/api';
import { selectedByLawColor$, selectedByLawData$, selectedByLawName$, setByLawData, setByLawName, setByLawZoneColor } from 'mods/bindings';
import { ByLawItem, ByLawItemType } from 'mods/types';
import { ConstraintListItem } from 'mods/components/ConstraintListItem/ConstraintListItem';
import { ChangeEvent, useEffect, useMemo, useState } from 'react';
import { TextInputTheme } from 'mods/components/TextInput/TextInput';
import classNames from 'classnames';
import { VanillaComponentResolver } from 'vanillacomponentresolver';
import { Color } from 'cs2/bindings';
import * as utils from 'mods/utils';

export const ByLawEditorView = ({ searchQuery }: { searchQuery?: string }) => {
    let byLawData = useValue(selectedByLawData$);
    let byLawName = useValue(selectedByLawName$);
    let byLawColor = useValue(selectedByLawColor$);
    let [_byLawName, set_ByLawName] = useState(byLawName);
    let [colorText, setColorText] = useState(utils.rgbaToHex(byLawColor[0]));

    useEffect(() => {
        set_ByLawName(byLawName);
    }, [byLawName]);
    useEffect(() => {
        setColorText(utils.rgbaToHex(byLawColor[0]));
    }, [byLawColor[0]]);


    let onNameChange = ({ target }: ChangeEvent<HTMLInputElement>) => {
        setByLawName(target.value);
    }

    let onColorChange = (col: Color) => {
        setByLawZoneColor(col, byLawColor[1]);
    }
    let onColorChangeHex = ({ target }: ChangeEvent<HTMLInputElement>) => {
        let hex = target.value;
        try {
            let newColor = utils.hexToRGBA(hex);
            onColorChange(newColor);
        } catch {
            // Was an error converting to hex, so ignoring the change
        }
    }

    let onConstraintUpdate = (newItemValue: ByLawItem) => {
        let nByLawData = utils.deepCopy(byLawData);
        let itemData = nByLawData.blocks[0].itemData;
        nByLawData.blocks[0].itemData = itemData.map(item => item.byLawItemType == newItemValue.byLawItemType ? newItemValue : item);
        setByLawData(nByLawData);
    }
    let onChangeConstraintEnabled = (newEnabledValue: boolean, itemType: ByLawItemType) => {
        let nByLawData = utils.deepCopy(byLawData);
        if (newEnabledValue) {
            nByLawData.blocks[0].itemData.push({
                ...utils.GetDefaultByLawItem(),
                byLawItemType: itemType,
                itemCategory: utils.getItemCategories(itemType),
                constraintType: utils.getConstraintTypes(itemType)[0],
                propertyOperator: utils.getDefaultPropertyOperator(itemType)
            });
        } else {
            nByLawData.blocks[0].itemData = nByLawData.blocks[0].itemData.filter((item) => item.byLawItemType != itemType);
        }
        setByLawData(nByLawData);
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
        .filter(([key, readableName]) => searchQuery && readableName ? readableName.toUpperCase().indexOf(searchQuery.toUpperCase()) >= 0 : true);
    let listItems = types.map(([key, readableName]: [keyof typeof ByLawItemType, string], idx) =>
        <ConstraintListItem
            key={idx}
            readableName={readableName}
            itemType={ByLawItemType[key]}
            value={itemMap[key] || undefined}
            onValueChange={onConstraintUpdate}
            onChangeConstraintEnabled={onChangeConstraintEnabled}
        />
    );

    return (
        <Scrollable className={styles.view} vertical trackVisibility='always'>
            <div className={classNames(styles.nameItem, { [styles.invisible]: searchQuery ? "NAME".indexOf(searchQuery.toUpperCase()) < 0 : false })}>
                <label>Name</label>
                <input
                    type={'text'}
                    value={_byLawName}
                    className={classNames(TextInputTheme.input, styles.textBox)}
                    onChange={({ target }) => set_ByLawName(target.value)}
                    onBlur={onNameChange}
                />
            </div>
            <div className={classNames(styles.colorItem, { [styles.invisible]: searchQuery ? "COLOR".indexOf(searchQuery.toUpperCase()) < 0 || "COLOUR".indexOf(searchQuery.toUpperCase()) < 0 : false })}>
                <label>Colour</label>
                <div>
                    <VanillaComponentResolver.instance.ColorField
                        value={byLawColor[0]}
                        onChange={onColorChange}
                        className={styles.colorButton}
                    />
                    <input
                        type={'text'}
                        value={colorText}
                        className={classNames(TextInputTheme.input, styles.textBox)}
                        onChange={({ target }) => setColorText(target.value)}
                        onBlur={onColorChangeHex}
                    />
                </div>
            </div>
            {listItems}
        </Scrollable>
    )
}