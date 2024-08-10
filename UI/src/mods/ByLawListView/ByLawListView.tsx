import { useValue } from 'cs2/api';
import styles from './ByLawListView.module.scss';
import { byLawZoneList$ } from 'mods/bindings';
import { ByLawZoneListItem } from 'mods/types';
import { Scrollable } from 'cs2/ui';
import { ByLawListItem } from 'mods/components/ByLawListItem/ByLawListItem';
import { entityKey } from 'cs2/utils';

export const ByLawListView = ({ searchQuery }: { searchQuery?: string }) => {
    let bylawList = useValue(byLawZoneList$);
    let listItems = bylawList
        .filter((item: ByLawZoneListItem) => searchQuery ? item.name.toUpperCase().indexOf(searchQuery!.toUpperCase()) > 0 : true)
        .map((item: ByLawZoneListItem) => <ByLawListItem item={item} key={entityKey(item.entity)} />);

    return (
        <Scrollable className={styles.view} vertical>
            {listItems}
        </Scrollable>
    );
}