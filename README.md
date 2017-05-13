EFQuery - A class for constructing serializable Entity Framework query objects.

=================

I've been working with a group that had been using its own home-grown ORM system, for many years. It had been written long before any of the modern ORMs existed - not only before Hibernate was released, but before Java was first publicly released. It had worked for us reasonably well, across a number of versions of languages, operating systems, compilers, databases, etc., but it had been getting long in the tooth, so we made the decision to move our database access to Entity Framework.

Entity Framework works very well, for much of what we do, but there were a couple of things we needed to be able to do that how to accomplish wasn't immediately obvious. In particular:

building up complex queries in code, where logic controls which pieces are included in the query
serializing queries into strings, so they can be stored, retrieved, loaded at runtime, passed from client to server, etc.
We found that it was possible to construct queries piecemeal, in the simplest cases, by chaining expression trees, but this fell fall short of what we needed.

Some searching led us to Joseph and Ben Albahari's PredicateBuilder. We found that didn't work well with Entity Framework, and that led us to Pete Montgomery's A universal PredicateBuilder, which did solve the problem for us.

That still left us with the problem of serializing queries, and that led us to implement EFQuery.

It's turned out to be useful in ways we'd not anticipated.

The code project is here: https://github.com/jdege/EFQuery

Documentation is here: https://github.com/jdege/EFQuery/wiki/Documentation

A unit test project is here: https://github.com/jdege/EFQueryTest

I'm working on a demonstration project, explaining what it can do and why. That will be here: https://github.com/jdege/EFQueryDemo


