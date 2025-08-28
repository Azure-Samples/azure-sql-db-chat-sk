delete from [web].[sessions_abstracts_embeddings];
delete from [web].[sessions_details_embeddings];
delete from [web].[sessions];
go

insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 9,
            "title": "Azure SQL and SQL Server: All Things Developers",
            "abstract": "Over the past two decades, SQL Server has undergone a remarkable evolution, emerging as the most widely deployed database in the world. A great number of new features have been announced for Azure SQL and SQL Server to support developers in being more efficient and productive when creating new solutions and applications or when modernizing existing ones. In this session, we go over all the lastest released features  such as JSON, Data API builder, calling REST endpoints, Azure function integrations and much more, so that you''ll learn how to take advantage of them right away.",
            "external_id": "683984",
            "details": {"speakers":["Davide Mauri"],"track":"Data Engineering","language":"English","level":300}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 2,
            "title": "Erkläre meiner techniknahen Frau Fabric und seine Fähigkeit, Daten sofort zu analysieren",
            "abstract": "Das Erlernen einer neuen Technologie wie Fabric ähnelt dem Erlernen einer neuen Sprache. Es gibt unbekannte Wörter, Grammatik und Konzepte, aber vieles ist ähnlich zu dem, was du bereits kennst. Wenn du dich im Umfeld von Microsoft Data befindest, bin ich mir sicher, dass du gefragt wurdest: Was ist Microsoft Fabric? Und Wofür können wir es verwenden? So haben meine techniknahe Ehefrau und ich diese Fragen geklärt und die Sprachbarriere überwunden :-) Wir werden auch zeigen, wie du Echtzeitanalysen über dein Smart Home in Microsoft Fabric abfragen kannst. Also, wenn du keine Ahnung hast, was Fabric ist, oder wenn du Fabric jemandem erklären musst, der keine Ahnung hat, könnten diese 10 Minuten nützlich für dich sein.  Explaining Fabric and its ability to analyse data instantly to my tech-adjacent wife  Learning a new technology like Fabric is similar learning a new language. There are unfamiliar words, grammar, and concepts but a lot is similar to what you already know. If you are around Microsoft Data I am certain that you will have been asked. What is Microsoft Fabric ? And What can we use it for? This is how my tech-adjacent wife, and I resolved those questions and broke down the language barrier :-) We will also show how you can query Real-Time Analytics about your smart home in Microsoft Fabric. So if you have no idea what Fabric is or you have to explain Fabric to someone who has no idea maybe these 10 minutes may be useful for you.",
            "external_id": "456",
            "details": {"speakers":["Rob Sewell"],"track":"Analytics & Data Science","language":"German","level":100}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

/* The next are random sessions generate via AI */

-- Session 3: Quantum Computing Fundamentals
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 3,
            "title": "Quantum Computing: From Theory to Practice",
            "abstract": "Quantum computing represents a paradigm shift in computational capabilities, promising to solve problems that are intractable for classical computers. This session explores the fundamental principles of quantum mechanics as applied to computing, including qubits, superposition, and quantum entanglement. We will examine current quantum hardware platforms, programming languages like Q#, and practical applications in cryptography, optimization, and machine learning. Attendees will gain insights into the current state of quantum computing and its potential impact on various industries.",
            "external_id": "QC2024-001",
            "details": {"speakers":["Dr. Sarah Chen"],"track":"Emerging Technologies","language":"English","level":400}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

-- Session 4: Sustainable Agriculture Technology
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 4,
            "title": "IoT Solutions for Precision Agriculture and Sustainability",
            "abstract": "Modern agriculture faces unprecedented challenges with climate change, population growth, and resource scarcity. This session explores how Internet of Things (IoT) technologies are revolutionizing farming practices through precision agriculture. We will cover soil monitoring sensors, drone-based crop surveillance, automated irrigation systems, and AI-powered crop prediction models. Learn how farmers are reducing water usage by 30% and increasing yields by 25% using smart farming techniques. Real-world case studies from vertical farms and traditional agriculture will be presented.",
            "external_id": "AGR2024-IoT",
            "details": {"speakers":["Maria Rodriguez","James Thompson"],"track":"IoT & Sustainability","language":"English","level":200}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

-- Session 5: Renaissance Art History and Digital Preservation
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 5,
            "title": "Digital Humanities: Preserving Renaissance Masterpieces with 3D Scanning",
            "abstract": "The intersection of art history and cutting-edge technology opens new possibilities for cultural preservation and education. This session demonstrates how high-resolution 3D scanning, photogrammetry, and virtual reality are being used to digitally preserve Renaissance paintings and sculptures. We will explore projects from the Vatican Museums, the Louvre, and the Uffizi Gallery where masterpieces by Leonardo da Vinci, Michelangelo, and Botticelli are being captured in unprecedented detail. Learn about the technical challenges of scanning delicate artworks and how these digital twins enable virtual museum experiences and art restoration research.",
            "external_id": "ARTTECH-REN",
            "details": {"speakers":["Dr. Alessandro Benedetti","Sophie Laurent"],"track":"Digital Humanities","language":"French","level":300}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

-- Session 6: Space Exploration Mission Planning
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 6,
            "title": "Mars Mission Logistics: AI-Powered Resource Management for Deep Space",
            "abstract": "Planning a human mission to Mars involves complex logistical challenges that push the boundaries of current technology and resource management. This session explores how artificial intelligence and machine learning are being used to optimize spacecraft trajectories, manage life support systems, and plan resource allocation for multi-year missions. We will examine NASA''s current Mars mission planning software, SpaceX''s Starship logistics systems, and the role of autonomous decision-making in environments where Earth communication delays can exceed 20 minutes. Topics include in-situ resource utilization, emergency contingency planning, and psychological support systems for isolated crews.",
            "external_id": "SPACE-MARS-2024",
            "details": {"speakers":["Dr. Michael Anderson","Dr. Yuki Tanaka"],"track":"Aerospace Engineering","language":"English","level":500}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

-- Session 7: Marine Biology and Ocean Conservation
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 7,
            "title": "Coral Reef Restoration: Biotechnology Meets Marine Conservation",
            "abstract": "Coral reefs are among Earth''s most biodiverse ecosystems, yet they face unprecedented threats from climate change, ocean acidification, and pollution. This session presents cutting-edge biotechnology approaches to coral reef restoration, including coral probiotics, assisted gene flow, and 3D-printed reef structures. We will explore successful restoration projects in the Great Barrier Reef, Caribbean, and Pacific regions. Learn about innovative techniques such as coral spawning synchronization, stress-resistant coral breeding, and the use of underwater drones for reef monitoring. The session includes case studies showing how technology is helping restore damaged reef ecosystems and protect marine biodiversity.",
            "external_id": "MARINE-BIO-CR",
            "details": {"speakers":["Dr. Elena Vasquez","Prof. David Kim"],"track":"Environmental Science","language":"Spanish","level":300}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

-- Session 8: Medieval History and Archaeology
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 8,
            "title": "LiDAR Archaeology: Uncovering Hidden Medieval Settlements",
            "abstract": "Light Detection and Ranging (LiDAR) technology is revolutionizing archaeological discovery by revealing hidden structures beneath forest canopies and agricultural fields. This session explores recent discoveries of medieval settlements, castles, and trade routes across Europe using airborne LiDAR surveys. We will examine case studies from England, Germany, and Poland where previously unknown medieval villages and fortifications have been discovered. Learn about the data processing techniques used to analyze LiDAR point clouds, integration with historical documents, and how these discoveries are reshaping our understanding of medieval settlement patterns and economic networks.",
            "external_id": "ARCH-LIDAR-MED",
            "details": {"speakers":["Prof. Margaret Sheffield","Dr. Heinrich Mueller"],"track":"Digital Archaeology","language":"German","level":250}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

-- Session 9: Culinary Science and Food Technology
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 10,
            "title": "Molecular Gastronomy: The Science Behind Modern Culinary Innovation",
            "abstract": "Molecular gastronomy represents the intersection of chemistry, physics, and culinary arts, transforming how we understand and experience food. This session explores the scientific principles behind techniques such as spherification, gelification, and emulsification that create unexpected textures and flavors. We will demonstrate equipment like liquid nitrogen, rotary evaporators, and ultrasonic baths used in modernist cuisine. Learn about the work of pioneering chefs like Ferran Adrià and Grant Achatz, and how scientific understanding of protein denaturation, Maillard reactions, and phase transitions is being applied to create innovative dining experiences. The session includes hands-on demonstrations of foam creation and flavor encapsulation.",
            "external_id": "GASTRO-MOL-SCI",
            "details": {"speakers":["Chef Isabella Romano","Dr. Pierre Dubois"],"track":"Food Science","language":"Italian","level":200}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

-- Session 10: Cognitive Psychology and Learning
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 11,
            "title": "Neuroscience of Learning: How the Brain Acquires New Skills",
            "abstract": "Understanding how the human brain learns and retains new information is crucial for education, training, and personal development. This session explores recent neuroscience research on learning mechanisms, including the role of neural plasticity, memory consolidation, and the default mode network. We will examine how different learning styles affect brain activation patterns, the impact of sleep on skill acquisition, and the neurological basis of expertise development. Topics include spaced repetition algorithms, the neuroscience of flow states, and how emerging technologies like transcranial stimulation might enhance learning. Real-world applications in education technology and professional training will be discussed.",
            "external_id": "NEURO-LEARN-2024",
            "details": {"speakers":["Dr. Rachel Thompson","Prof. Hiroshi Yamamoto"],"track":"Cognitive Science","language":"Japanese","level":350}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

-- Session 11: Music Technology and Sound Engineering
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 12,
            "title": "Spatial Audio and 3D Sound Design for Virtual Reality",
            "abstract": "The future of audio experiences lies in three-dimensional soundscapes that respond to user movement and create immersive environments. This session explores the technical foundations of spatial audio, including binaural recording techniques, head-related transfer functions (HRTFs), and real-time audio processing algorithms. We will demonstrate how game engines like Unity and Unreal implement 3D audio, examine the challenges of low-latency audio rendering for VR headsets, and explore applications in gaming, virtual concerts, and therapeutic environments. Learn about the latest advances in personalized HRTF generation, the use of AI for dynamic sound scene adaptation, and how spatial audio is transforming entertainment and communication.",
            "external_id": "AUDIO-3D-VR",
            "details": {"speakers":["Alex Rivera","Dr. Emma Johnson"],"track":"Audio Technology","language":"English","level":400}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

-- Session 12: Urban Planning and Smart Cities
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 13,
            "title": "Digital Twins for Urban Planning: Simulating City-Scale Infrastructure",
            "abstract": "Modern cities are complex systems requiring sophisticated modeling and simulation tools for effective planning and management. This session explores how digital twin technology is being used to create virtual replicas of entire cities, enabling planners to test infrastructure changes, predict traffic patterns, and optimize resource allocation before implementation. We will examine real-world projects from Singapore, Amsterdam, and Toronto where digital twins are being used for flood management, energy optimization, and transportation planning. Learn about the integration of IoT sensors, satellite imagery, and citizen-generated data to create accurate city models, and how machine learning is being used to predict urban growth patterns and infrastructure needs.",
            "external_id": "URBAN-DT-2024",
            "details": {"speakers":["Dr. Priya Sharma","Lars Andersen"],"track":"Smart Cities","language":"Dutch","level":300}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

-- Session 13: Linguistics and Language Technology
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 14,
            "title": "Computational Linguistics: Preserving Endangered Languages with AI",
            "abstract": "With over 3,000 languages at risk of extinction within the next century, computational linguistics is playing a crucial role in language preservation and revitalization efforts. This session explores how natural language processing, speech recognition, and machine learning are being used to document endangered languages, create digital dictionaries, and develop language learning applications. We will examine projects working with indigenous communities in Australia, the Americas, and Africa to digitize oral traditions and create interactive language resources. Learn about the challenges of working with low-resource languages, the ethical considerations of language documentation, and how AI-powered tools are helping communities maintain their linguistic heritage while bridging communication gaps with the digital world.",
            "external_id": "LING-AI-PRES",
            "details": {"speakers":["Dr. Amara Okafor","Prof. Standing Bear"],"track":"Computational Linguistics","language":"English","level":275}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

-- Session 14: Biomechanics and Sports Science
insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 15,
            "title": "Motion Capture Analytics: Optimizing Athletic Performance Through Biomechanics",
            "abstract": "Modern sports science leverages advanced motion capture technology and biomechanical analysis to optimize athletic performance and prevent injuries. This session explores how high-speed cameras, force plates, and wearable sensors are used to analyze movement patterns in elite athletes across various sports. We will demonstrate real-time gait analysis, jumping mechanics assessment, and throwing technique optimization using 3D motion capture systems. Learn about the integration of machine learning algorithms that can predict injury risk based on movement patterns, the development of personalized training programs based on biomechanical data, and how professional sports teams are using this technology to gain competitive advantages. Case studies from Olympic training centers and professional football, basketball, and tennis programs will be presented.",
            "external_id": "SPORTS-BIOMECH",
            "details": {"speakers":["Dr. Maria Santos","Coach Kevin Mitchell"],"track":"Sports Science","language":"Portuguese","level":325}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

select * from web.sessions
go

-- Generate embeddings for all sessions 
declare @session_id int;
declare @abstract nvarchar(max);
declare @details nvarchar(max);
declare @abstract_embedding vector(1536);
declare @details_embedding vector(1536);

declare session_cursor cursor for
    select id, abstract, cast(details as nvarchar(max))
    from [web].[sessions];

open session_cursor;

fetch next from session_cursor into @session_id, @abstract, @details;

while @@FETCH_STATUS = 0
begin
    -- Generate embedding for abstract
    exec web.get_embedding @abstract, @abstract_embedding output;
    insert into [web].[sessions_abstracts_embeddings] ([session_id], abstract_vector_text3) 
    values (@session_id, @abstract_embedding);
    
    -- Generate embedding for details
    exec web.get_embedding @details, @details_embedding output;
    insert into [web].[sessions_details_embeddings] ([session_id], details_vector_text3) 
    values (@session_id, @details_embedding);
    
    fetch next from session_cursor into @session_id, @abstract, @details;
end

close session_cursor;
deallocate session_cursor;
go

select * from web.sessions_abstracts_embeddings
select * from web.sessions_details_embeddings
go