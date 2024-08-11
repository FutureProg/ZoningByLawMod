import { Scrollable } from 'cs2/ui';
import styles from './ByLawEditorView.module.scss';
import { useValue } from 'cs2/api';
import { selectedByLawColor$, selectedByLawData$, selectedByLawName$ } from 'mods/bindings';
import { ByLawItemType } from 'mods/types';
import { ConstraintListItem } from 'mods/components/ConstraintListItem/ConstraintListItem';
import { useMemo } from 'react';

export const ByLawEditorView = ({ searchQuery }: { searchQuery?: string }) => {
    let byLawData = useValue(selectedByLawData$);
    let byLawName = useValue(selectedByLawName$);
    let byLawColor = useValue(selectedByLawColor$);

    let items = byLawData.blocks[0].itemData;
    let itemMap = useMemo(() =>
        Object.fromEntries(
            items.map((item) => [ByLawItemType[item.byLawItemType], item])
        )
    , [items]);
    console.log(itemMap, Object.keys(itemMap));    
    let types = Object.keys(ByLawItemType)        
        .filter((key) => isNaN(Number(key)) && key != 'None')
        .map((key) => [key, key.split(/(?<![A-Z])(?=[A-Z])/).join(' ')] as [keyof typeof ByLawItemType, string])        
        .filter(([key, readableName]) => searchQuery ? readableName.toUpperCase().indexOf(searchQuery.toUpperCase()) > 0 : true);
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
            {listItems}
        </Scrollable>
    )
}