import { Scrollable } from 'cs2/ui';
import styles from './ByLawEditorView.module.scss';
import { useValue } from 'cs2/api';
import { selectedByLawColor$, selectedByLawData$, selectedByLawName$ } from 'mods/bindings';
import { ByLawItemType } from 'mods/types';
import { ConstraintListItem } from 'mods/components/ConstraintListItem/ConstraintListItem';

export const ByLawEditorView = ({searchQuery} : {searchQuery?: string}) => {
    let byLawData = useValue(selectedByLawData$);
    let byLawName = useValue(selectedByLawName$);
    let byLawColor = useValue(selectedByLawColor$);

    let types = Object.keys(ByLawItemType)        
        .filter((val) => isNaN(Number(val)) && val != 'None')        
        .map((val) => val.split(/(?<![A-Z])(?=[A-Z])/).join(' '))
        .filter((val) => searchQuery? val.toUpperCase().indexOf(searchQuery.toUpperCase()) > 0 : true);
    let listItems = types.map((typeInfo, idx) => 
        <ConstraintListItem 
            key={idx} 
            itemType={typeInfo as any as (ByLawItemType)}
            value={undefined}
        />
    );

    return (
        <Scrollable className={styles.view}>
            {listItems}
        </Scrollable>
    )
}