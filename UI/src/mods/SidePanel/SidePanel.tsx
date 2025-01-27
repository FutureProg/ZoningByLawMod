import { ByLawListView } from 'mods/ByLawListView/ByLawListView';
import styles from './SidePanel.module.scss';
import { useEffect, useState } from 'react';
import { SidePanelHeader } from 'mods/SidePanelHeader/SidePanelHeader';
import { SidePanelViews } from 'mods/types';
import { ByLawEditorView } from 'mods/ByLawEditorView/ByLawEditorView';
import { useValue } from 'cs2/api';
import { createNewByLaw, deleteByLaw, elligibleBuildingCount$, selectedByLaw$ } from 'mods/bindings';
import { createPortal } from 'react-dom';
import { Tooltip } from 'cs2/ui';
import { useLocalization } from 'cs2/l10n';
import classNames from 'classnames';
import { CounterTile } from 'mods/components/CounterTile';

import CityIcon from '../../images/city-icon.svg'

export const SidePanel = () => {
    let { translate } = useLocalization();
    let [currentView, setCurrentView] = useState<SidePanelViews>('bylaws');
    let [searchQuery, setSearchQuery] = useState<string | undefined>();
    let activeByLaw = useValue(selectedByLaw$);
    let elligibleBuildings = useValue(elligibleBuildingCount$);

    let onSearchChange = (text: string) => {
        setSearchQuery(text);
    }
    let onViewChange = (newView: SidePanelViews) => {
        setCurrentView(newView);
    }

    let onCreateByLaw = () => {
        createNewByLaw();
    }
    let onDeleteByLaw = () => {  
        deleteByLaw();                
    }

    useEffect(() => {
        if (activeByLaw.index > 0) {
            setCurrentView('editor');
        } else {
            setCurrentView('bylaws');
        }
    }, [activeByLaw.index]);

    let editorButtons = currentView == 'editor' ? (
        <>        
        {/* <Tooltip tooltip={translate("ZBL.Tooltip[Duplicate]", "Duplicate (Not Implemented)")} direction='right'>
            <div className={classNames(styles.sideButton)}>
                <img src="coui://uil/Dark/RectangleCopy.svg" />
            </div>
        </Tooltip> */}
        <div className={styles.divider}></div>
        <Tooltip tooltip={translate("Common.DELETE_TOOLTIP", "Delete")} direction='right'>
            <div onClick={onDeleteByLaw} className={classNames(styles.sideButton, styles.warningButton)}>
                <img src="coui://uil/Dark/Trash.svg" />
            </div>
        </Tooltip>
        {/* <div className={styles.divider}></div>
        <Tooltip tooltip={translate("ZBL.Tooltip[PreviewZoneButton]", "Preview Zone (Not Implemented)")} direction='right'>
            <div className={styles.sideButton}>
                <img src="coui://uil/Dark/Cube.svg" />
            </div>
        </Tooltip>                     */}
        </>
    ) : <></>;

    let sideButtons = createPortal((
        <div className={styles.sideButtons}>
            {currentView == 'editor'? <CounterTile 
                value={elligibleBuildings} 
                thresholds={{danger: 2, warning: 20}} 
                iconSrc={CityIcon}
                hint={translate("ZBL.Tooltip[EligibleBuildingCount]", "The number of elligible buildings") as string} /> : null}
            <Tooltip tooltip={translate("ZBL.Tooltip[CreateNewByLaw]", "Create A New ByLaw")} direction='right'>
                <div onClick={onCreateByLaw} className={styles.sideButton}>
                    <img src="coui://uil/Dark/Plus.svg" />
                </div>
            </Tooltip>
            {editorButtons}
        </div>
    ), document.body);

    return (
        <div className={styles.view}>
            <SidePanelHeader currentView={currentView} onSearchQueryChange={onSearchChange} onViewChange={onViewChange} />
            {currentView == 'bylaws' ? <ByLawListView searchQuery={searchQuery} /> : <ByLawEditorView searchQuery={searchQuery} selectedByLaw={activeByLaw} />}
            {sideButtons}
        </div>
    )
}