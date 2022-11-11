const createPersonResultType = exports.createPersonResultType = {
    success: 0,
    conflict: 1,
    validationFailure: 2,
    badRequest: 3,
    serverError: 4,
    unknownError: 5,
};

exports.createPerson = async (post, personToCreate) => {
    const response = await post('/api/people/create', personToCreate);

    if (response.status == 201) {
        return {
            tag: createPersonResultType.success,
            createdPerson: response.data,
        };
    }

    if (response.status == 400 && response.data) {
        return {
            tag: createPersonResultType.validationFailure,
            validationErrors: response.data,
        };
    }

    if (response.status == 409) {
        return {
            tag: createPersonResultType.conflict,
        };
    }

    if (response.status == 500) {
        return {
            tag: createPersonResultType.serverError,
        };
    }

    return {
        tag: createPersonResultType.unknownError,
    };
}