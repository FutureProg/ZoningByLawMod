import { useLocalization } from 'cs2/l10n';
import styles from './SidePanelHeader.module.scss';
import { SidePanelViews } from 'mods/types';
import classNames from 'classnames';
import { SearchTextBox } from 'mods/components/SearchTextBox/SearchTextBox';
import { useValue } from 'cs2/api';
import { selectedByLaw$ } from 'mods/bindings';

type SidePanelHeaderProps = {
    currentView: SidePanelViews;
    searchQuery?: string;

    onSearchQueryChange: (text: string) => void;
    onViewChange: (newView: SidePanelViews) => void;
}

export const SidePanelHeader = (props: SidePanelHeaderProps) => {
    let { translate } = useLocalization();
    let activeByLaw = useValue(selectedByLaw$);

    let editorClassNames = classNames(styles.viewButton, {
         [styles.selected]: props.currentView == 'editor',
         [styles.disabled]: activeByLaw.index <= 0
    });
    return (
        <div className={styles.view}>
            <div className={styles.title}>{translate('ZBL.ByLawPanel[Title]', "Zoning By Laws")}</div>
            <div className={styles.buttonRow}>
                <div
                    className={classNames(styles.viewButton, { [styles.selected]: props.currentView == 'bylaws' })}
                    onClick={() => props.onViewChange('bylaws')}
                >
                    {translate('ZBL.ByLawPanel[ByLawList]', "Your ByLaws")}
                </div>
                <div
                    className={editorClassNames}
                    onClick={() => activeByLaw.index <= 0? null : props.onViewChange('editor')}
                >
                    {translate('ZBL.ByLawPanel[Editor]', "Editor")}
                </div>
            </div>
            <div>
                <SearchTextBox onChange={props.onSearchQueryChange} value={props.searchQuery} />
            </div>
        </div>
    )

}