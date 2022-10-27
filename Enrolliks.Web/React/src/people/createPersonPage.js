const React = require('react');
const axios = require('axios').default;

class CreatePersonPage extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            name: '',
            submitAllowed: true,
            error: false
        };
    }

    async submit() {
        try {
            this.setState({submitAllowed: false});

            await axios.post('/people/create', {
                name: this.state.name
            });

            this.props.goBack();
        } catch {
            this.setState({submitAllowed: true, error: true});
        }
    }
    
    render() {
        const errorStyle = {
            display: this.state.error ? 'inline' : 'none'
        };

        const handleNameChange = event => this.setState({name: event.target.value});
        const handleButtonClick = () => this.submit();

        return (
            <div>
                <label>Name</label>
                <span style={errorStyle}>Error</span>
                <input type={'text'} value={this.state.name} onChange={handleNameChange}></input>
                <button onClick={handleButtonClick} disabled={!this.state.submitAllowed}>Create</button>
            </div>
        );
    }
}

module.exports = CreatePersonPage;