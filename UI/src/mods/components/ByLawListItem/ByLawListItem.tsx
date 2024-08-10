import { ByLawZoneListItem } from 'mods/types';
import styles from './ByLawListItem.module.scss';
import { useValue } from 'cs2/api';
import { selectedByLaw$, setActiveByLaw } from 'mods/bindings';
import classNames from 'classnames';

export const ByLawListItem = ({ item }: { item: ByLawZoneListItem }) => {
    let selectedByLaw = useValue(selectedByLaw$);
    let className = classNames(styles.panelListItem, {
        [styles.selected]: selectedByLaw.index == item.entity.index
    });
    return (
        <div onClick={() => setActiveByLaw(item.entity)} className={className} key={item.entity.index}>
            <div className={styles.byLawName}>
                {item.name}
            </div>
        </div>
    )
}