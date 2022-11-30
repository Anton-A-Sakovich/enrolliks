const { useState } = require('react');
const React = require('react');
const ImageButton = require('../imageButton');
const logic = require('./personRowLogic');
const { deletePerson: deletePersonEndpoint } = require('./personEndpoints');
const axios = require('axios').default;
const xmarkRegular = require('../images/circle-xmark-regular.svg');
const xmarkSolid = require('../images/circle-xmark-solid.svg');
const xmarkSolidGray = require('../images/circle-xmark-solid-gray.svg');

module.exports = function PersonRow(props) {
    const [isConfirmationActive, setIsConfirmationActive] = useState(false);

    const content = isConfirmationActive
        ? <DeleteConfirmation
            person={props.person}
            onCompleted={props.onRemoved}
            onCancelled={() => setIsConfirmationActive(false)} />
        : <PersonInformation
            person={props.person}
            onRemoveClicked={() => setIsConfirmationActive(true)} />;

    return <tr>{content}</tr>;
};

function PersonInformation (props) {
    return (
        <td>
            {props.person.name}
            <ImageButton
                className='remove-button'
                onClick={props.onRemoveClicked}
                src={xmarkRegular}
                hoverSrc={xmarkSolid}
                downSrc={xmarkSolidGray} />
        </td>
    );
}

const del = (url) => axios({
    url: url,
    method: 'delete',
    validateStatus: null,
}).then(response => ({
    status: response.status,
    data: (response.data === undefined || response.data === null) ? null : response.data,
}));

const endpoint = personName => deletePersonEndpoint(del, personName);

class DeleteConfirmation extends React.Component {
    constructor(props) {
        super(props);
        this.state = logic.initialize();
    }

    render() {
        const title = this.state.requestInProgress
            ? 'Deleting...'
            : this.state.requestStatus.length > 0
                ? this.state.requestStatus
                : `Are you sure you want to remove ${this.props.person.name}`;

        const buttonsOrLoader = this.state.requestInProgress
            ? <span className='inline-loader' />
            : (
                <>
                    <button onClick={this.handleRemoveClick}>
                        Remove
                    </button>
                    <button onClick={this.handleCancelClick}>
                        Cancel
                    </button>
                </>
            );

        return <td>{title}{buttonsOrLoader}</td>;
    }

    handleRemoveClick = async () => {
        const [stateUpdateBeforeRequest, continuation] = logic.delete(endpoint, this.state, this.props.person.name);
        
        this.setState(stateUpdateBeforeRequest);

        if (continuation) {
            const stateUpdateAfterRequest = await continuation();
            this.setState(stateUpdateAfterRequest);

            if (stateUpdateAfterRequest.requestStatus.length === 0) {
                this.props.onCompleted();
            }
        }
    };

    handleCancelClick = () => {
        this.props.onCancelled();
    }
}